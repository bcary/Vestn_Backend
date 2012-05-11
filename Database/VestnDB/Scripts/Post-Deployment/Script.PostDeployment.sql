/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

INSERT INTO [dbo].[aspnet_Applications]
VALUES('/','/','D82BE5F9-B014-41E9-B6E2-C489F519719A',NULL)

INSERT INTO dbo.aspnet_SchemaVersions
VALUES
('common', 1, 1),
('health monitoring', 1, 1),
('membership', 1, 1),
('personalization', 1, 1),
('profile', 1, 1),
('role manager', 1, 1);