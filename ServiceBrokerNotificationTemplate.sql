USE [DatabaseName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-----------------------------------------------
-----------------------------------------------
-- find and replace: b67e1033_057a_48a1_a40f_89438359f115_Name
-----------------------------------------------
-----------------------------------------------


-----------------------------------------------
-----------------------------------------------
-- UNINSTALL PROCEDURE
-----------------------------------------------
-----------------------------------------------

IF OBJECT_ID ('dbo.sp_UninstallListenerNotification_b67e1033_057a_48a1_a40f_89438359f115_Name', 'P') IS NOT NULL
	DROP PROCEDURE dbo.sp_UninstallListenerNotification_b67e1033_057a_48a1_a40f_89438359f115_Name

GO

CREATE PROCEDURE dbo.sp_UninstallListenerNotification_b67e1033_057a_48a1_a40f_89438359f115_Name
AS
BEGIN
	IF OBJECT_ID ('dbo.tr_Listener_b67e1033_057a_48a1_a40f_89438359f115_Name', 'TR') IS NOT NULL
		DROP TRIGGER dbo.[tr_Listener_b67e1033_057a_48a1_a40f_89438359f115_Name]

	-- Service Broker uninstall statement.
                            
    DECLARE @serviceId INT
    SELECT @serviceId = service_id FROM sys.services 
    WHERE sys.services.name = 'ListenerService_b67e1033_057a_48a1_a40f_89438359f115_Name'

    DECLARE @ConvHandle uniqueidentifier
    DECLARE Conv CURSOR FOR
    SELECT CEP.conversation_handle FROM sys.conversation_endpoints CEP
    WHERE CEP.service_id = @serviceId AND ([state] != 'CD' OR [lifetime] > GETDATE() + 1)

    OPEN Conv;
    FETCH NEXT FROM Conv INTO @ConvHandle;
    WHILE (@@FETCH_STATUS = 0) BEGIN
    	END CONVERSATION @ConvHandle WITH CLEANUP;
        FETCH NEXT FROM Conv INTO @ConvHandle;
    END
    CLOSE Conv;
    DEALLOCATE Conv;

	-- Droping service and queue.
    IF (@serviceId IS NOT NULL)
        DROP SERVICE [ListenerService_b67e1033_057a_48a1_a40f_89438359f115_Name]

    IF OBJECT_ID ('dbo.ListenerQueue_b67e1033_057a_48a1_a40f_89438359f115_Name', 'SQ') IS NOT NULL
	    DROP QUEUE dbo.[ListenerQueue_b67e1033_057a_48a1_a40f_89438359f115_Name]
END

GO
  
-----------------------------------------------
-----------------------------------------------
-- INSTALL PROCEDURE
-----------------------------------------------
-----------------------------------------------

IF OBJECT_ID ('dbo.sp_InstallListenerNotification_b67e1033_057a_48a1_a40f_89438359f115_Name', 'P') IS NOT NULL
	DROP PROCEDURE dbo.sp_InstallListenerNotification_b67e1033_057a_48a1_a40f_89438359f115_Name

GO

CREATE PROCEDURE dbo.sp_InstallListenerNotification_b67e1033_057a_48a1_a40f_89438359f115_Name
AS
BEGIN
	IF OBJECT_ID ('dbo.sp_UninstallListenerNotification_b67e1033_057a_48a1_a40f_89438359f115_Name', 'P') IS NOT NULL
		EXEC [dbo].[sp_UninstallListenerNotification_b67e1033_057a_48a1_a40f_89438359f115_Name]

	-- Create a queue which will hold the tracked information 
	IF NOT EXISTS (SELECT * FROM sys.service_queues WHERE name = 'ListenerQueue_b67e1033_057a_48a1_a40f_89438359f115_Name')
		CREATE QUEUE dbo.[ListenerQueue_b67e1033_057a_48a1_a40f_89438359f115_Name]
	-- Create a service on which tracked information will be sent 
	IF NOT EXISTS(SELECT * FROM sys.services WHERE name = 'ListenerService_b67e1033_057a_48a1_a40f_89438359f115_Name')
		CREATE SERVICE [ListenerService_b67e1033_057a_48a1_a40f_89438359f115_Name] ON QUEUE dbo.[ListenerQueue_b67e1033_057a_48a1_a40f_89438359f115_Name] ([DEFAULT])                  
		
	IF OBJECT_ID ('dbo.tr_Listener_b67e1033_057a_48a1_a40f_89438359f115_Name', 'TR') IS NOT NULL
		DROP TRIGGER [tr_Listener_b67e1033_057a_48a1_a40f_89438359f115_Name]

	EXEC ('
		CREATE TRIGGER [tr_Listener_b67e1033_057a_48a1_a40f_89438359f115_Name]
		ON dbo.Deliveries
		AFTER INSERT, UPDATE
		AS
		BEGIN
			SET NOCOUNT ON;

			IF EXISTS (SELECT * FROM sys.services WHERE name = ''ListenerService_b67e1033_057a_48a1_a40f_89438359f115_Name'')
			BEGIN
				DECLARE @message NVARCHAR(MAX)

				SET @message = (
					SELECT Id AS [Id]
					FROM inserted
					FOR JSON PATH
				)

				IF(@message IS NOT NULL)
				BEGIN
					--Beginning of dialog...
					DECLARE @ConvHandle UNIQUEIDENTIFIER
					--Determine the Initiator Service, Target Service and the Contract 
					BEGIN DIALOG @ConvHandle 
						FROM SERVICE [ListenerService_b67e1033_057a_48a1_a40f_89438359f115_Name] TO SERVICE ''ListenerService_b67e1033_057a_48a1_a40f_89438359f115_Name'' ON CONTRACT [DEFAULT] WITH ENCRYPTION=OFF, LIFETIME = 60; 
					--Send the Message
					SEND ON CONVERSATION @ConvHandle MESSAGE TYPE [DEFAULT] (@message);
					--End conversation
					END CONVERSATION @ConvHandle;
				END
			END
		END
	')
End

GO



-----------------------------------------------
-----------------------------------------------
-- EXEC INSTALL
-----------------------------------------------
-----------------------------------------------
EXEC [dbo].[sp_InstallListenerNotification_b67e1033_057a_48a1_a40f_89438359f115_Name]