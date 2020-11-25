CREATE PROCEDURE [dbo].[P_<tabla>_query]
	@<PK> int = NULL
AS
	SELECT <campos>
	FROM <tabla>
	WHERE @<PK> IS NULL OR <PK> = @<PK>
GO
