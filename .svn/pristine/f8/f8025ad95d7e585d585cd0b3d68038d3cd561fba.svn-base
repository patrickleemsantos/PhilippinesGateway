Imports System.Data.SqlClient
Imports System.Xml
Imports System.Configuration

Public Class SQLHelper
    'The SQLHelper class is intended to encapsulate high performance, scalable best practices for 
    'common uses of SqlClient
    Public Shared Function GetConnectionString() As String

        'Get the DB Connection String from web.config

        Return ConfigurationManager.AppSettings("ConnectionString").ToString()
    End Function

#Region "private utility methods & constructors"

    ' Since this class provides only static methods, make the default constructor private to prevent 
    ' instances from being created with "new SQLHelper()"
    Private Sub New()
    End Sub

    ''' <summary>
    ''' This method is used to attach array of SqlParameters to a SqlCommand.
    ''' 
    ''' This method will assign a value of DbNull to any parameter with a direction of
    ''' InputOutput and a value of null.  
    ''' 
    ''' This behavior will prevent default values from being used, but
    ''' this will be the less common case than an intended pure output parameter (derived as InputOutput)
    ''' where the user provided no input value.
    ''' </summary>
    ''' <param name="command">The command to which the parameters will be added</param>
    ''' <param name="commandParameters">An array of SqlParameters to be added to command</param>
    Private Shared Sub AttachParameters(ByVal command As SqlCommand, ByVal commandParameters As SqlParameter())
        If command Is Nothing Then
            Throw New ArgumentNullException("command")
        End If
        If commandParameters IsNot Nothing Then
            For Each p As SqlParameter In commandParameters
                If p IsNot Nothing Then
                    ' Check for derived output value with no value assigned
                    If (p.Direction = ParameterDirection.InputOutput OrElse p.Direction = ParameterDirection.Input) AndAlso (p.Value Is Nothing) Then
                        p.Value = DBNull.Value
                    End If
                    command.Parameters.Add(p)
                End If
            Next
        End If
    End Sub

    ''' <summary>
    ''' This method assigns dataRow column values to an array of SqlParameters
    ''' </summary>
    ''' <param name="commandParameters">Array of SqlParameters to be assigned values</param>
    ''' <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
    Private Shared Sub AssignParameterValues(ByVal commandParameters As SqlParameter(), ByVal dataRow As DataRow)
        If (commandParameters Is Nothing) OrElse (dataRow Is Nothing) Then
            ' Do nothing if we get no data
            Return
        End If

        Dim i As Integer = 0
        ' Set the parameters values
        For Each commandParameter As SqlParameter In commandParameters
            ' Check the parameter name
            If commandParameter.ParameterName Is Nothing OrElse commandParameter.ParameterName.Length <= 1 Then
                Throw New Exception(String.Format("Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", i, commandParameter.ParameterName))
            End If
            If dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) <> -1 Then
                commandParameter.Value = dataRow(commandParameter.ParameterName.Substring(1))
            End If
            i += 1
        Next
    End Sub

    ''' <summary>
    ''' This method assigns an array of values to an array of SqlParameters
    ''' </summary>
    ''' <param name="commandParameters">Array of SqlParameters to be assigned values</param>
    ''' <param name="parameterValues">Array of objects holding the values to be assigned</param>
    Private Shared Sub AssignParameterValues(ByVal commandParameters As SqlParameter(), ByVal parameterValues As Object())
        If (commandParameters Is Nothing) OrElse (parameterValues Is Nothing) Then
            ' Do nothing if we get no data
            Return
        End If

        ' We must have the same number of values as we pave parameters to put them in
        If commandParameters.Length <> parameterValues.Length Then
            Throw New ArgumentException("Parameter count does not match Parameter Value count.")
        End If

        ' Iterate through the SqlParameters, assigning the values from the corresponding position in the 
        ' value array
        Dim i As Integer = 0, j As Integer = commandParameters.Length
        While i < j
            ' If the current array value derives from IDbDataParameter, then assign its Value property
            If TypeOf parameterValues(i) Is IDbDataParameter Then
                Dim paramInstance As IDbDataParameter = DirectCast(parameterValues(i), IDbDataParameter)
                If paramInstance.Value Is Nothing Then
                    commandParameters(i).Value = DBNull.Value
                Else
                    commandParameters(i).Value = paramInstance.Value
                End If
            ElseIf parameterValues(i) Is Nothing Then
                commandParameters(i).Value = DBNull.Value
            Else
                commandParameters(i).Value = parameterValues(i)
            End If
            i += 1
        End While
    End Sub

    ''' <summary>
    ''' This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
    ''' to the provided command
    ''' </summary>
    ''' <param name="command">The SqlCommand to be prepared</param>
    ''' <param name="connection">A valid SqlConnection, on which to execute this command</param>
    ''' <param name="transaction">A valid SqlTransaction, or 'null'</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
    ''' <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwose is false.</param>
    Private Shared Sub PrepareCommand(ByVal command As SqlCommand, ByVal connection As SqlConnection, ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String, ByVal commandParameters As SqlParameter(), _
     ByVal mustCloseConnection As Boolean)
        If command Is Nothing Then
            Throw New ArgumentNullException("command")
        End If
        If commandText Is Nothing OrElse commandText.Length = 0 Then
            Throw New ArgumentNullException("commandText")
        End If

        ' If the provided connection is not open, we will open it
        If connection.State <> ConnectionState.Open Then
            mustCloseConnection = True
            connection.Open()
        Else
            mustCloseConnection = False
        End If

        ' Associate the connection with the command
        command.Connection = connection

        ' Set the command text (stored procedure name or SQL statement)
        command.CommandText = commandText

        ' If we were provided a transaction, assign it
        If transaction IsNot Nothing Then
            If transaction.Connection Is Nothing Then
                Throw New ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction")
            End If
            command.Transaction = transaction
        End If

        ' Set the command type
        command.CommandType = commandType

        ' Attach the command parameters if they are provided
        If commandParameters IsNot Nothing Then
            AttachParameters(command, commandParameters)
        End If
        Return
    End Sub

#End Region

#Region "ExecuteNonQuery"

    ''' <summary>
    ''' Execute a SqlCommand (that returns no resultset and takes no parameters) against the database specified in 
    ''' the connection string
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
    ''' </remarks>
    ''' <param name="connectionString">A valid connection string for a SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>An int representing the number of rows affected by the command</returns>
    Public Shared Function ExecuteNonQuery(ByVal connectionString As String, ByVal commandType As CommandType, ByVal commandText As String) As Integer
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteNonQuery(connectionString, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
    ''' using the provided parameters
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connectionString">A valid connection string for a SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>An int representing the number of rows affected by the command</returns>
    Public Shared Function ExecuteNonQuery(ByVal connectionString As String, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As Integer
        If connectionString Is Nothing OrElse connectionString.Length = 0 Then
            Throw New ArgumentNullException("connectionString")
        End If

        ' Create & open a SqlConnection, and dispose of it after we are done
        Using connection As New SqlConnection(connectionString)
            connection.Open()

            ' Call the overload that takes a connection in place of the connection string
            Return ExecuteNonQuery(connection, commandType, commandText, commandParameters)
        End Using
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlConnection. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>An int representing the number of rows affected by the command</returns>
    Public Shared Function ExecuteNonQuery(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String) As Integer
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteNonQuery(connection, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns no resultset) against the specified SqlConnection 
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>An int representing the number of rows affected by the command</returns>
    Public Shared Function ExecuteNonQuery(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As Integer
        If connection Is Nothing Then
            Throw New ArgumentNullException("connection")
        End If

        ' Create a command and prepare it for execution
        Dim cmd As New SqlCommand()
        Dim mustCloseConnection As Boolean = False
        PrepareCommand(cmd, connection, DirectCast(Nothing, SqlTransaction), commandType, commandText, commandParameters, _
         mustCloseConnection)

        ' Finally, execute the command
        Dim retval As Integer = cmd.ExecuteNonQuery()

        ' Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear()
        If mustCloseConnection Then
            connection.Close()
        End If
        Return retval
    End Function



    ''' <summary>
    ''' Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>An int representing the number of rows affected by the command</returns>
    Public Shared Function ExecuteNonQuery(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String) As Integer
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteNonQuery(transaction, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns no resultset) against the specified SqlTransaction
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>An int representing the number of rows affected by the command</returns>
    Public Shared Function ExecuteNonQuery(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As Integer
        If transaction Is Nothing Then
            Throw New ArgumentNullException("transaction")
        End If
        If transaction IsNot Nothing AndAlso transaction.Connection Is Nothing Then
            Throw New ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction")
        End If

        ' Create a command and prepare it for execution
        Dim cmd As New SqlCommand()
        Dim mustCloseConnection As Boolean = False
        PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, _
         mustCloseConnection)

        ' Finally, execute the command
        Dim retval As Integer = cmd.ExecuteNonQuery()

        ' Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear()
        Return retval
    End Function


#End Region

#Region "ExecuteDataset"

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
    ''' the connection string. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
    ''' </remarks>
    ''' <param name="connectionString">A valid connection string for a SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>A dataset containing the resultset generated by the command</returns>
    Public Shared Function ExecuteDataset(ByVal connectionString As String, ByVal commandType As CommandType, ByVal commandText As String) As DataSet
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteDataset(connectionString, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connectionString">A valid connection string for a SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>A dataset containing the resultset generated by the command</returns>
    Public Shared Function ExecuteDataset(ByVal connectionString As String, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As DataSet
        If connectionString Is Nothing OrElse connectionString.Length = 0 Then
            Throw New ArgumentNullException("connectionString")
        End If

        ' Create & open a SqlConnection, and dispose of it after we are done
        Using connection As New SqlConnection(connectionString)
            connection.Open()

            ' Call the overload that takes a connection in place of the connection string
            Return ExecuteDataset(connection, commandType, commandText, commandParameters)
        End Using
    End Function


    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>A dataset containing the resultset generated by the command</returns>
    Public Shared Function ExecuteDataset(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String) As DataSet
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteDataset(connection, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>A dataset containing the resultset generated by the command</returns>
    Public Shared Function ExecuteDataset(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As DataSet
        If connection Is Nothing Then
            Throw New ArgumentNullException("connection")
        End If

        ' Create a command and prepare it for execution
        Dim cmd As New SqlCommand()
        Dim mustCloseConnection As Boolean = False
        PrepareCommand(cmd, connection, DirectCast(Nothing, SqlTransaction), commandType, commandText, commandParameters, _
         mustCloseConnection)

        ' Create the DataAdapter & DataSet
        Using da As New SqlDataAdapter(cmd)
            Dim ds As New DataSet()

            ' Fill the DataSet using default values for DataTable names, etc
            da.Fill(ds)

            ' Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()

            If mustCloseConnection Then
                connection.Close()
            End If

            ' Return the dataset
            Return ds
        End Using
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>A dataset containing the resultset generated by the command</returns>
    Public Shared Function ExecuteDataset(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String) As DataSet
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteDataset(transaction, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>A dataset containing the resultset generated by the command</returns>
    Public Shared Function ExecuteDataset(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As DataSet
        If transaction Is Nothing Then
            Throw New ArgumentNullException("transaction")
        End If
        If transaction IsNot Nothing AndAlso transaction.Connection Is Nothing Then
            Throw New ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction")
        End If

        ' Create a command and prepare it for execution
        Dim cmd As New SqlCommand()
        Dim mustCloseConnection As Boolean = False
        PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, _
         mustCloseConnection)

        ' Create the DataAdapter & DataSet
        Using da As New SqlDataAdapter(cmd)
            Dim ds As New DataSet()

            ' Fill the DataSet using default values for DataTable names, etc
            da.Fill(ds)

            ' Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()

            ' Return the dataset
            Return ds
        End Using
    End Function



#End Region

#Region "ExecuteReader"

    ''' <summary>
    ''' This enum is used to indicate whether the connection was provided by the caller, or created by SqlHelper, so that
    ''' we can set the appropriate CommandBehavior when calling ExecuteReader()
    ''' </summary>
    Private Enum SqlConnectionOwnership
        ''' <summary>Connection is owned and managed by SqlHelper</summary>
        Internal
        ''' <summary>Connection is owned and managed by the caller</summary>
        External
    End Enum

    ''' <summary>
    ''' Create and prepare a SqlCommand, and call ExecuteReader with the appropriate CommandBehavior.
    ''' </summary>
    ''' <remarks>
    ''' If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
    ''' 
    ''' If the caller provided the connection, we want to leave it to them to manage.
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection, on which to execute this command</param>
    ''' <param name="transaction">A valid SqlTransaction, or 'null'</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
    ''' <param name="connectionOwnership">Indicates whether the connection parameter was provided by the caller, or created by SqlHelper</param>
    ''' <returns>SqlDataReader containing the results of the command</returns>
    Private Shared Function ExecuteReader(ByVal connection As SqlConnection, ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String, ByVal commandParameters As SqlParameter(), ByVal connectionOwnership As SqlConnectionOwnership) As SqlDataReader
        If connection Is Nothing Then
            Throw New ArgumentNullException("connection")
        End If

        Dim mustCloseConnection As Boolean = False
        ' Create a command and prepare it for execution
        Dim cmd As New SqlCommand()
        Try
            PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, _
             mustCloseConnection)

            ' Create a reader
            Dim dataReader As SqlDataReader

            ' Call ExecuteReader with the appropriate CommandBehavior
            If connectionOwnership = SqlConnectionOwnership.External Then
                dataReader = cmd.ExecuteReader()
            Else
                dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection)
            End If

            ' Detach the SqlParameters from the command object, so they can be used again.
            ' HACK: There is a problem here, the output parameter values are fletched 
            ' when the reader is closed, so if the parameters are detached from the command
            ' then the SqlReader can´t set its values. 
            ' When this happen, the parameters can´t be used again in other command.
            Dim canClear As Boolean = True
            For Each commandParameter As SqlParameter In cmd.Parameters
                If commandParameter.Direction <> ParameterDirection.Input Then
                    canClear = False
                End If
            Next

            If canClear Then
                cmd.Parameters.Clear()
            End If

            Return dataReader
        Catch
            If mustCloseConnection Then
                connection.Close()
            End If
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
    ''' the connection string. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
    ''' </remarks>
    ''' <param name="connectionString">A valid connection string for a SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>A SqlDataReader containing the resultset generated by the command</returns>
    Public Shared Function ExecuteReader(ByVal connectionString As String, ByVal commandType As CommandType, ByVal commandText As String) As SqlDataReader
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteReader(connectionString, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connectionString">A valid connection string for a SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>A SqlDataReader containing the resultset generated by the command</returns>
    Public Shared Function ExecuteReader(ByVal connectionString As String, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As SqlDataReader
        If connectionString Is Nothing OrElse connectionString.Length = 0 Then
            Throw New ArgumentNullException("connectionString")
        End If
        Dim connection As SqlConnection = Nothing
        Try
            connection = New SqlConnection(connectionString)
            connection.Open()

            ' Call the private overload that takes an internally owned connection in place of the connection string
            Return ExecuteReader(connection, Nothing, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal)
        Catch
            ' If we fail to return the SqlDatReader, we need to close the connection ourselves
            If connection IsNot Nothing Then
                connection.Close()
            End If
            Throw
        End Try

    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>A SqlDataReader containing the resultset generated by the command</returns>
    Public Shared Function ExecuteReader(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String) As SqlDataReader
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteReader(connection, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>A SqlDataReader containing the resultset generated by the command</returns>
    Public Shared Function ExecuteReader(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As SqlDataReader
        ' Pass through the call to the private overload using a null transaction value and an externally owned connection
        Return ExecuteReader(connection, DirectCast(Nothing, SqlTransaction), commandType, commandText, commandParameters, SqlConnectionOwnership.External)
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>A SqlDataReader containing the resultset generated by the command</returns>
    Public Shared Function ExecuteReader(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String) As SqlDataReader
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteReader(transaction, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''   SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>A SqlDataReader containing the resultset generated by the command</returns>
    Public Shared Function ExecuteReader(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As SqlDataReader
        If transaction Is Nothing Then
            Throw New ArgumentNullException("transaction")
        End If
        If transaction IsNot Nothing AndAlso transaction.Connection Is Nothing Then
            Throw New ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction")
        End If

        ' Pass through to private overload, indicating that the connection is owned by the caller
        Return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External)
    End Function

#End Region

#Region "ExecuteScalar"

    ''' <summary>
    ''' Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
    ''' the connection string. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
    ''' </remarks>
    ''' <param name="connectionString">A valid connection string for a SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    Public Shared Function ExecuteScalar(ByVal connectionString As String, ByVal commandType As CommandType, ByVal commandText As String) As Object
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteScalar(connectionString, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string 
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connectionString">A valid connection string for a SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    Public Shared Function ExecuteScalar(ByVal connectionString As String, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As Object
        If connectionString Is Nothing OrElse connectionString.Length = 0 Then
            Throw New ArgumentNullException("connectionString")
        End If
        ' Create & open a SqlConnection, and dispose of it after we are done
        Using connection As New SqlConnection(connectionString)
            connection.Open()

            ' Call the overload that takes a connection in place of the connection string
            Return ExecuteScalar(connection, commandType, commandText, commandParameters)
        End Using
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlConnection. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    Public Shared Function ExecuteScalar(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String) As Object
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteScalar(connection, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    Public Shared Function ExecuteScalar(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As Object
        If connection Is Nothing Then
            Throw New ArgumentNullException("connection")
        End If

        ' Create a command and prepare it for execution
        Dim cmd As New SqlCommand()

        Dim mustCloseConnection As Boolean = False
        PrepareCommand(cmd, connection, DirectCast(Nothing, SqlTransaction), commandType, commandText, commandParameters, _
         mustCloseConnection)

        ' Execute the command & return the results
        Dim retval As Object = cmd.ExecuteScalar()

        ' Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear()

        If mustCloseConnection Then
            connection.Close()
        End If

        Return retval
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    Public Shared Function ExecuteScalar(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String) As Object
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteScalar(transaction, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
    Public Shared Function ExecuteScalar(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As Object
        If transaction Is Nothing Then
            Throw New ArgumentNullException("transaction")
        End If
        If transaction IsNot Nothing AndAlso transaction.Connection Is Nothing Then
            Throw New ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction")
        End If

        ' Create a command and prepare it for execution
        Dim cmd As New SqlCommand()
        Dim mustCloseConnection As Boolean = False
        PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, _
         mustCloseConnection)

        ' Execute the command & return the results
        Dim retval As Object = cmd.ExecuteScalar()

        ' Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear()
        Return retval
    End Function

#End Region

#Region "ExecuteXmlReader"
    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
    ''' <returns>An XmlReader containing the resultset generated by the command</returns>
    Public Shared Function ExecuteXmlReader(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String) As XmlReader
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteXmlReader(connection, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>An XmlReader containing the resultset generated by the command</returns>
    Public Shared Function ExecuteXmlReader(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As XmlReader
        If connection Is Nothing Then
            Throw New ArgumentNullException("connection")
        End If

        Dim mustCloseConnection As Boolean = False
        ' Create a command and prepare it for execution
        Dim cmd As New SqlCommand()
        Try
            PrepareCommand(cmd, connection, DirectCast(Nothing, SqlTransaction), commandType, commandText, commandParameters, _
             mustCloseConnection)

            ' Create the DataAdapter & DataSet
            Dim retval As XmlReader = cmd.ExecuteXmlReader()

            ' Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()

            Return retval
        Catch
            If mustCloseConnection Then
                connection.Close()
            End If
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
    ''' <returns>An XmlReader containing the resultset generated by the command</returns>
    Public Shared Function ExecuteXmlReader(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String) As XmlReader
        ' Pass through the call providing null for the set of SqlParameters
        Return ExecuteXmlReader(transaction, commandType, commandText, DirectCast(Nothing, SqlParameter()))
    End Function

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <returns>An XmlReader containing the resultset generated by the command</returns>
    Public Shared Function ExecuteXmlReader(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String, ByVal ParamArray commandParameters As SqlParameter()) As XmlReader
        If transaction Is Nothing Then
            Throw New ArgumentNullException("transaction")
        End If
        If transaction IsNot Nothing AndAlso transaction.Connection Is Nothing Then
            Throw New ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction")
        End If

        ' Create a command and prepare it for execution
        Dim cmd As New SqlCommand()
        Dim mustCloseConnection As Boolean = False
        PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, _
         mustCloseConnection)

        ' Create the DataAdapter & DataSet
        Dim retval As XmlReader = cmd.ExecuteXmlReader()

        ' Detach the SqlParameters from the command object, so they can be used again
        cmd.Parameters.Clear()
        Return retval
    End Function


#End Region

#Region "FillDataset"
    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
    ''' the connection string. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
    ''' </remarks>
    ''' <param name="connectionString">A valid connection string for a SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    ''' <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    ''' by a user defined name (probably the actual table name)</param>
    Public Shared Sub FillDataset(ByVal connectionString As String, ByVal commandType As CommandType, ByVal commandText As String, ByVal dataSet As DataSet, ByVal tableNames As String())
        If connectionString Is Nothing OrElse connectionString.Length = 0 Then
            Throw New ArgumentNullException("connectionString")
        End If
        If dataSet Is Nothing Then
            Throw New ArgumentNullException("dataSet")
        End If

        ' Create & open a SqlConnection, and dispose of it after we are done
        Using connection As New SqlConnection(connectionString)
            connection.Open()

            ' Call the overload that takes a connection in place of the connection string
            FillDataset(connection, commandType, commandText, dataSet, tableNames)
        End Using
    End Sub

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connectionString">A valid connection string for a SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    ''' <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    ''' <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    ''' by a user defined name (probably the actual table name)
    ''' </param>
    Public Shared Sub FillDataset(ByVal connectionString As String, ByVal commandType As CommandType, ByVal commandText As String, ByVal dataSet As DataSet, ByVal tableNames As String(), ByVal ParamArray commandParameters As SqlParameter())
        If connectionString Is Nothing OrElse connectionString.Length = 0 Then
            Throw New ArgumentNullException("connectionString")
        End If
        If dataSet Is Nothing Then
            Throw New ArgumentNullException("dataSet")
        End If
        ' Create & open a SqlConnection, and dispose of it after we are done
        Using connection As New SqlConnection(connectionString)
            connection.Open()

            ' Call the overload that takes a connection in place of the connection string
            FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters)
        End Using
    End Sub

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    ''' <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    ''' by a user defined name (probably the actual table name)
    ''' </param>    
    Public Shared Sub FillDataset(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String, ByVal dataSet As DataSet, ByVal tableNames As String())
        FillDataset(connection, commandType, commandText, dataSet, tableNames, Nothing)
    End Sub

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    ''' <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    ''' by a user defined name (probably the actual table name)
    ''' </param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    Public Shared Sub FillDataset(ByVal connection As SqlConnection, ByVal commandType As CommandType, ByVal commandText As String, ByVal dataSet As DataSet, ByVal tableNames As String(), ByVal ParamArray commandParameters As SqlParameter())
        FillDataset(connection, Nothing, commandType, commandText, dataSet, tableNames, _
         commandParameters)
    End Sub

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    ''' <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    ''' by a user defined name (probably the actual table name)
    ''' </param>
    Public Shared Sub FillDataset(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String, ByVal dataSet As DataSet, ByVal tableNames As String())
        FillDataset(transaction, commandType, commandText, dataSet, tableNames, Nothing)
    End Sub

    ''' <summary>
    ''' Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    ''' <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    ''' by a user defined name (probably the actual table name)
    ''' </param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    Public Shared Sub FillDataset(ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String, ByVal dataSet As DataSet, ByVal tableNames As String(), ByVal ParamArray commandParameters As SqlParameter())
        FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, _
         commandParameters)
    End Sub


    ''' <summary>
    ''' Private helper method that execute a SqlCommand (that returns a resultset) against the specified SqlTransaction and SqlConnection
    ''' using the provided parameters.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
    ''' </remarks>
    ''' <param name="connection">A valid SqlConnection</param>
    ''' <param name="transaction">A valid SqlTransaction</param>
    ''' <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
    ''' <param name="commandText">The stored procedure name or T-SQL command</param>
    ''' <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
    ''' <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
    ''' by a user defined name (probably the actual table name)
    ''' </param>
    ''' <param name="commandParameters">An array of SqlParamters used to execute the command</param>
    Private Shared Sub FillDataset(ByVal connection As SqlConnection, ByVal transaction As SqlTransaction, ByVal commandType As CommandType, ByVal commandText As String, ByVal dataSet As DataSet, ByVal tableNames As String(), _
     ByVal ParamArray commandParameters As SqlParameter())
        If connection Is Nothing Then
            Throw New ArgumentNullException("connection")
        End If
        If dataSet Is Nothing Then
            Throw New ArgumentNullException("dataSet")
        End If

        ' Create a command and prepare it for execution
        Dim command As New SqlCommand()
        Dim mustCloseConnection As Boolean = False
        PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, _
         mustCloseConnection)

        ' Create the DataAdapter & DataSet
        Using dataAdapter As New SqlDataAdapter(command)

            ' Add the table mappings specified by the user
            If tableNames IsNot Nothing AndAlso tableNames.Length > 0 Then
                Dim tableName As String = "Table"
                For index As Integer = 0 To tableNames.Length - 1
                    If tableNames(index) Is Nothing OrElse tableNames(index).Length = 0 Then
                        Throw New ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames")
                    End If
                    dataAdapter.TableMappings.Add(tableName, tableNames(index))
                    tableName += (index + 1).ToString()
                Next
            End If

            ' Fill the DataSet using default values for DataTable names, etc
            dataAdapter.Fill(dataSet)

            ' Detach the SqlParameters from the command object, so they can be used again
            command.Parameters.Clear()
        End Using

        If mustCloseConnection Then
            connection.Close()
        End If
    End Sub
#End Region

#Region "UpdateDataset"
    ''' <summary>
    ''' Executes the respective command for each inserted, updated, or deleted row in the DataSet.
    ''' </summary>
    ''' <remarks>
    ''' e.g.:  
    '''  UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
    ''' </remarks>
    ''' <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source</param>
    ''' <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source</param>
    ''' <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source</param>
    ''' <param name="dataSet">The DataSet used to update the data source</param>
    ''' <param name="tableName">The DataTable used to update the data source.</param>
    Public Shared Sub UpdateDataset(ByVal insertCommand As SqlCommand, ByVal deleteCommand As SqlCommand, ByVal updateCommand As SqlCommand, ByVal dataSet As DataSet, ByVal tableName As String)
        If insertCommand Is Nothing Then
            Throw New ArgumentNullException("insertCommand")
        End If
        If deleteCommand Is Nothing Then
            Throw New ArgumentNullException("deleteCommand")
        End If
        If updateCommand Is Nothing Then
            Throw New ArgumentNullException("updateCommand")
        End If
        If tableName Is Nothing OrElse tableName.Length = 0 Then
            Throw New ArgumentNullException("tableName")
        End If

        ' Create a SqlDataAdapter, and dispose of it after we are done
        Using dataAdapter As New SqlDataAdapter()
            ' Set the data adapter commands
            dataAdapter.UpdateCommand = updateCommand
            dataAdapter.InsertCommand = insertCommand
            dataAdapter.DeleteCommand = deleteCommand

            ' Update the dataset changes in the data source
            dataAdapter.Update(dataSet, tableName)

            ' Commit all the changes made to the DataSet
            dataSet.AcceptChanges()
        End Using
    End Sub
#End Region

#Region "Check Null"
    Public Shared Function CheckDateNull(ByVal readerValue As Object) As Object
        If (readerValue.Equals(DBNull.Value)) OrElse (readerValue Is Nothing) Then
            Return Nothing
        Else
            Return readerValue
        End If
    End Function
    Public Shared Function CheckIntNull(ByVal readerValue As Object) As Integer
        If (readerValue.Equals(DBNull.Value)) OrElse (readerValue Is Nothing) Then
            Return -1
        Else
            Return Convert.ToInt32(readerValue)
        End If
    End Function
    Public Shared Function CheckStringNull(ByVal readerValue As Object) As String
        If (readerValue.Equals(DBNull.Value)) OrElse (String.IsNullOrEmpty(readerValue.ToString())) Then
            Return ""
        Else
            Return Convert.ToString(readerValue)
        End If
    End Function
    Public Shared Function CheckFloatNull(ByVal readerValue As Object) As Single
        If (readerValue.Equals(DBNull.Value)) OrElse (readerValue Is Nothing) Then
            Return -1.0F
        Else
            Return Convert.ToSingle(readerValue)
        End If
    End Function
    Public Shared Function CheckBooleanNull(ByVal readerValue As Object) As Boolean
        If (readerValue.Equals(DBNull.Value)) OrElse (readerValue Is Nothing) Then
            Return False
        Else
            Return Convert.ToBoolean(readerValue)
        End If
    End Function
#End Region

End Class
Friend NotInheritable Class SqlValueConverter
    Private Sub New()
    End Sub

#Region "getters based on ordinals"
    Public Shared Function GetString(ByVal reader As IDataReader, ByVal ordinal As Integer) As [String]
        If reader.IsDBNull(ordinal) Then
            Return [String].Empty
        End If
        Return reader.GetString(ordinal)
    End Function
    Public Shared Function GetInt32(ByVal reader As IDataReader, ByVal ordinal As Integer) As Integer
        If reader.IsDBNull(ordinal) Then
            Return 0
        End If
        Return Convert.ToInt32(reader.GetValue(ordinal))
    End Function
    Public Shared Function GetInt64(ByVal reader As IDataReader, ByVal ordinal As Integer) As Long
        If reader.IsDBNull(ordinal) Then
            Return 0
        End If
        Return Convert.ToInt64(reader.GetValue(ordinal))
    End Function
    Public Shared Function GetDateTime(ByVal reader As IDataReader, ByVal ordinal As Integer) As DateTime
        If reader.IsDBNull(ordinal) Then
            Return DateTime.MinValue
        End If
        Return Convert.ToDateTime(reader.GetValue(ordinal))
    End Function
#End Region

End Class
'end class
