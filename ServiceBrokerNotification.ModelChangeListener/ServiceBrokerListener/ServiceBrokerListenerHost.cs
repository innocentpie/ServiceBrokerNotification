using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ServiceBrokerNotification.ModelChangeListener.ServiceBrokerListener;

public class ServiceBrokerListenerHost<T> : IModelChangeListenerHost<T>
{
    /// <summary>
    /// T-SQL script-template which helps to receive changed data in monitorable table.
    /// {0} - database name.
    /// {1} - schema name.
    /// {2} - conversation queue name.
    /// {3} - timeout.
    /// </summary>
    private const string SQL_FORMAT_RECEIVE_EVENT = @"
            DECLARE @ConvHandle UNIQUEIDENTIFIER
            DECLARE @message VARBINARY(MAX)
            USE [{0}]
            WAITFOR (RECEIVE TOP(1) @ConvHandle=Conversation_Handle
                        , @message=message_body FROM {1}.[{2}]), TIMEOUT {3};
	        BEGIN TRY END CONVERSATION @ConvHandle; END TRY BEGIN CATCH END CATCH

            SELECT CAST(@message AS NVARCHAR(MAX)) 
        ";

    /// <summary>
    /// T-SQL script-template which helps to clear the service broker queue.
    /// {0} - database name.
    /// {1} - schema name.
    /// {2} - conversation queue name.
    /// </summary>
    private const string SQL_FORMAT_CLEAR_QUEUE = @"
            USE [{0}]
            DECLARE @conversationHandle UNIQUEIDENTIFIER
            
            DECLARE convCursor CURSOR
                LOCAL STATIC FORWARD_ONLY READ_ONLY
                FOR
                    SELECT conversation_handle 
	                FROM {1}.[{2}] WITH (NOLOCK)
	                WHERE validation = 'N'

            BEGIN
                OPEN convCursor
                FETCH NEXT FROM convCursor INTO @conversationHandle
                WHILE @@FETCH_STATUS = 0 
                BEGIN
                    BEGIN TRY END CONVERSATION @conversationHandle WITH CLEANUP; END TRY BEGIN CATCH END CATCH
                    FETCH NEXT FROM convCursor INTO @conversationHandle
                END
                CLOSE convCursor
             END

            DEALLOCATE convCursor
        ";



    public bool Active { get; private set; }
    public string ConnectionString { get; private set; }
    public string DatabaseName { get; private set; }
    public string SchemaName { get; private set; }
    public string ConversationQueueName { get; private set; }
    public int CommandTimeout { get; private set; }
    public bool ClearQueueOnStartup { get; private set; }


    public event Action<T[]>? OnRecieveEvent;
    public event Action? OnListenerStarted;
    public event Action<Exception>? OnListenerStopped;

    public ServiceBrokerListenerHost(ServiceBrokerListenerHostOptions options)
    {
        ConnectionString = options.ConnectionString;
        ConversationQueueName = options.ConversationQueueName;
        DatabaseName = options.DatabaseName;
        SchemaName = options.SchemaName;
        CommandTimeout = options.CommandTimeout;
        ClearQueueOnStartup = options.ClearQueueOnStartup;
    }

    public async Task RunListener(CancellationToken cancellationToken)
    {
        try
        {
            if (ClearQueueOnStartup)
                await ClearQueue(cancellationToken);

            Active = true;
            OnListenerStarted?.Invoke();

            while (true)
            {
                var message = await ReceiveEvent(cancellationToken);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    JsonArray jsonArray = JsonNode.Parse(message).AsArray();
                    T[] array = jsonArray.Deserialize<T[]>();

                    OnRecieveEvent?.Invoke(array);
                }
            }
        }
        catch (Exception e)
        {
            Active = false;
            OnListenerStopped?.Invoke(e);
        }
    }

    private async Task<string> ReceiveEvent(CancellationToken token)
    {
        var commandText = string.Format(
            SQL_FORMAT_RECEIVE_EVENT,
            DatabaseName,
            SchemaName,
            ConversationQueueName,
            CommandTimeout / 2
        );


        using (SqlConnection conn = new SqlConnection(ConnectionString))
        using (SqlCommand command = new SqlCommand(commandText, conn))
        {
            await conn.OpenAsync(token);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = CommandTimeout;
            using (var reader = await command.ExecuteReaderAsync(token))
            {
                if (!await reader.ReadAsync(token) || reader.IsDBNull(0))
                    return null;

                return reader.GetString(0);
            }
        }
    }

    private async Task ClearQueue(CancellationToken token)
    {
        var commandText = string.Format(
            SQL_FORMAT_CLEAR_QUEUE,
            DatabaseName,
            SchemaName,
            ConversationQueueName
        );

        using (SqlConnection conn = new SqlConnection(ConnectionString))
        using (SqlCommand command = new SqlCommand(commandText, conn))
        {
            await conn.OpenAsync(token);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 0;
            await command.ExecuteNonQueryAsync(token);
        }
    }
}
