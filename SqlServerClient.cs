#region Licenses
/*MIT License
Copyright(c) 2020
Robert Garrison

Permission Is hereby granted, free Of charge, To any person obtaining a copy
of this software And associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
copies Of the Software, And To permit persons To whom the Software Is
furnished To Do so, subject To the following conditions:

The above copyright notice And this permission notice shall be included In all
copies Or substantial portions Of the Software.

THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
FITNESS For A PARTICULAR PURPOSE And NONINFRINGEMENT. In NO Event SHALL THE
AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
OUT Of Or In CONNECTION With THE SOFTWARE Or THE USE Or OTHER DEALINGS In THE
SOFTWARE*/
#endregion
#region Using Statements
using ADONetHelper.Core;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
#endregion

namespace ADONetHelper.SqlServer
{
    /// <summary>
    /// A specialized instance of <see cref="DbClient"/> that is used to query a SQL Server database system
    /// </summary>
    /// <seealso cref="DbClient"/>
    /// <seealso cref="IXMLExecutor"/>
    public class SqlServerClient : DbClient
    {
        #region Events
        /// <summary>
        /// Occurs when SQL Server returns a warning or informational message
        /// </summary>
        public event SqlInfoMessageEventHandler InfoMessage
        {
            add
            {
                //Get an exclusive lock first
                lock (ExecuteSQL.Connection)
                {
                    Connection.InfoMessage += value;
                }
            }
            remove
            {
                //Get an exclusive lock first
                lock (ExecuteSQL.Connection)
                {
                    Connection.InfoMessage -= value;
                }
            }
        }
        #endregion
        #region Fields/Properties
        /// <summary>
        /// An instance of <see cref="SqlConnection"/>
        /// </summary>
        /// <returns>Returns an instance of <see cref="SqlConnection"/></returns>
        protected SqlConnection Connection
        {
            get
            {
                //Return this back to the caller
                return (SqlConnection)ExecuteSQL.Connection;
            }
        }
        /// <summary>
        /// Enables statistics gathering for the current connection when set to <c>true</c>
        /// </summary>
        /// <returns>Returns <c>true</c> if statistics are enabled, <c>false</c> otherwise</returns>
        public bool StatisticsEnabled
        {
            get
            {
                //Return this back to the caller
                return Connection.StatisticsEnabled;
            }
            set
            {
                Connection.StatisticsEnabled = value;
            }
        }
        /// <summary>
        /// The size in bytes of network packets used to communicate with an instance of sql server
        /// </summary>
        /// <returns></returns>
        public int PacketSize
        {
            get
            {
                //Return this back to the caller
                return Connection.PacketSize;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether [fire information message event on user errors].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [fire information message event on user errors]; otherwise, <c>false</c>.
        /// </value>
        public bool FireInfoMessageEventOnUserErrors
        {
            get
            {
                return Connection.FireInfoMessageEventOnUserErrors;
            }
            set
            {
                Connection.FireInfoMessageEventOnUserErrors = value;
            }
        }
        /// <summary>
        /// Gets a string that identifies the database client.
        /// </summary>
        /// <returns>Gets a string that identifies the database client.</returns>
        public string WorkstationID
        {
            get
            {
                //Return this back to the caller
                return Connection.WorkstationId;
            }
        }
        /// <summary>
        /// Gets or sets the access token for the connection
        /// </summary>
        /// <returns>The access token as a <see cref="string"/></returns>
        public string AccessToken
        {
            get
            {
                //Return this back to the caller
                return Connection.AccessToken;
            }
            set
            {
                Connection.AccessToken = value;
            }
        }
        /// <summary>
        /// The connection ID of the most recent connection attempt, regardless of whether the attempt succeeded or failed.
        /// </summary>
        /// <returns>The connection ID of the most recent connection attempt, regardless of whether the attempt succeeded or failed as a <c>string</c></returns>
        public Guid ClientConnectionID
        {
            get
            {
                //Return this back to the caller
                return Connection.ClientConnectionId;
            }
        }
        #endregion
        #region Constructors
        /// <summary>
        /// Intializes the <see cref="SqlServerClient"/> with a <see cref="ISqlExecutor"/>
        /// </summary>
        /// <param name="executor">An instance of <see cref="ISqlExecutor"/></param>
        public SqlServerClient(ISqlExecutor executor) : base(executor)
        {
        }
        /// <summary>
        /// The overloaded constuctor that will initialize the <paramref name="connectionString"/>, And <paramref name="queryCommandType"/>
        /// </summary>
        /// <param name="connectionString">The connection string used to query a data store</param>
        /// <param name="queryCommandType">Represents how a command should be interpreted by the data provider</param>
        public SqlServerClient(string connectionString, CommandType queryCommandType) : base(connectionString, queryCommandType, SqlClientFactory.Instance)
        {
        }
        /// <summary>
        /// The overloaded constuctor that will initialize the <paramref name="connectionString"/>
        /// </summary>
        /// <param name="connectionString">The connection string used to query a data store</param>
        public SqlServerClient(string connectionString) : base(connectionString, SqlClientFactory.Instance)
        {
        }
        /// <summary>
        /// Constructor to query a database using an existing <see cref="SqlConnection"/> to initialize the <paramref name="connection"/>
        /// </summary>
        /// <param name="connection">An instance of <see cref="SqlConnection"/> to use to connect to a server and database</param>
        public SqlServerClient(SqlConnection connection) : base(connection)
        {
        }
        /// <summary>
        /// Insantiates a new instance of <see cref="SqlServerClient"/> using the passed in <paramref name="connectionString"/> and <paramref name="factory"/>
        /// </summary>
        /// <param name="connectionString">Connection string to use to query a database</param>
        /// <param name="factory">An instance of <see cref="IDbObjectFactory"/></param>
        public SqlServerClient(string connectionString, IDbObjectFactory factory) : base(connectionString, factory)
        {
        }
        #endregion
        #region Utility Methods
        /// <summary>
        /// Empties the connection pool associated with this instance of <see cref="SqlServerClient"/> <see cref="SqlConnection"/>
        /// </summary>
        /// <remarks>
        /// ClearPool clears the connection pool that is associated with the current <see cref="SqlConnection"/>. If additional connections associated with connection are in use at the time of the call, they are marked appropriately and are discarded (instead of being returned to the pool) when Close is called on them.
        /// </remarks>
        public void ClearPool()
        {
            //Clear the current pool
            SqlConnection.ClearPool(Connection);
        }
        /// <summary>
        /// Returns an instance of <see cref="XmlReader"/> based on the <paramref name="query"/>
        /// </summary>
        /// <param name="query">The query command text Or name of stored procedure to execute against the data store</param>
        /// <returns>Returns an instance of <see cref="XmlReader"/> based on the <paramref name="query"/> passed into the routine</returns>
        public XmlReader ExecuteXMLReader(string query)
        {
            //Wrap this in a using statement to automatically dispose of resources
            using (SqlCommand command = (SqlCommand)ExecuteSQL.Factory.GetDbCommand(QueryCommandType, query, ExecuteSQL.Parameters, Connection, CommandTimeout))
            {
                try
                {
                    //Return this back to the caller
                    return command.ExecuteXmlReader();
                }
                finally
                {
                    command.Parameters.Clear();
                }
            }
        }
        /// <summary>
        /// Returns an instance of <see cref="XmlReader"/> based on the <paramref name="query"/>
        /// </summary>
        /// <param name="token">Structure object that propagates notification that operations should be canceled.</param>
        /// <param name="query">The query command text Or name of stored procedure to execute against the data store</param>
        /// <returns>Returns an instance of <see cref="XmlReader"/> based on the <paramref name="query"/> passed into the routine</returns>
        public async Task<XmlReader> ExecuteXMLReaderAsync(string query, CancellationToken token = default)
        {
            //Wrap this in a using statement to automatically dispose of resources
            using (SqlCommand command = (SqlCommand)ExecuteSQL.Factory.GetDbCommand(QueryCommandType, query, ExecuteSQL.Parameters, Connection, CommandTimeout))
            {
                try
                {
                    //Return this back to the caller
                    return await command.ExecuteXmlReaderAsync(token).ConfigureAwait(false);
                }
                finally
                {
                    //Clear parameters
                    command.Parameters.Clear();
                }
            }
        }
        /// <summary>
        /// All statistics are set to zero if <see cref="SqlConnection.StatisticsEnabled"/> is <c>true</c>
        /// </summary>
        public void ResetStatistics()
        {
            Connection.ResetStatistics();
        }
        /// <summary>
        /// Gets an instance of <see cref="SqlBulkCopy"/> based off of the existing <see cref="SqlConnection"/> being used by the instance
        /// </summary>
        /// <returns>Returns an instance of <see cref="SqlBulkCopy"/> for the client to configure</returns>
        public SqlBulkCopy GetSQLBulkCopy()
        {
            //Return this back to the caller
            return new SqlBulkCopy(Connection);
        }
        /// <summary>
        /// Gets an instance of <see cref="SqlBulkCopy"/> using the passed in <paramref name="connectionString"/>
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the database as a <see cref="string"/></param>
        /// <returns>Returns an instance of <see cref="SqlBulkCopy"/></returns>
        public SqlBulkCopy GetSqlBulkCopy(string connectionString)
        {
            //Return this back to the caller
            return new SqlBulkCopy(connectionString);
        }
        /// <summary>
        /// Gets an instance of <see cref="SqlBulkCopy"/> using the passed in <paramref name="connectionString"/> and <paramref name="options"/>
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the database as a <see cref="string"/></param>
        /// <param name="options">The <see cref="SqlBulkCopyOptions"/> to configure the <see cref="SqlBulkCopy"/></param>
        /// <returns>Returns an instance of <see cref="SqlBulkCopy"/></returns>
        public SqlBulkCopy GetSqlBulkCopy(string connectionString, SqlBulkCopyOptions options)
        {
            //Return this back to the caller
            return new SqlBulkCopy(connectionString, options);
        }
        /// <summary>
        /// Gets an instance of <see cref="SqlBulkCopy"/> based off of the existing <see cref="SqlConnection"/> being used by the instance
        /// </summary>
        /// <param name="options">The <see cref="SqlBulkCopyOptions"/> to configure the <see cref="SqlBulkCopy"/></param>
        /// <param name="transaction">An instance of <see cref="SqlTransaction"/></param>
        /// <returns>Returns an instance of <see cref="SqlBulkCopy"/></returns>
        public SqlBulkCopy GetSQLBulkCopy(SqlBulkCopyOptions options, SqlTransaction transaction)
        {
            //Return this back to the caller
            return new SqlBulkCopy(Connection, options, transaction);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="newPassword"></param>
        public void ChangePassword(string connectionString, string newPassword)
        {
            SqlConnection.ChangePassword(connectionString, newPassword);
        }
        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="password">The password.</param>
        public void ChangePassword(string connectionString, SqlCredential credential, SecureString password)
        {
            SqlConnection.ChangePassword(connectionString, credential, password);
        }
        /// <summary>
        /// Returns a name value pair collection of statistics at the point in time the method is called
        /// </summary>
        /// <returns>Returns a reference of <see cref="IDictionary{TKey, TValue}"/> of <see cref="DictionaryEntry"/></returns>
        /// <remarks>When this method is called, the values retrieved are those at the current point in time. 
        /// If you continue using the connection, the values are incorrect. You need to re-execute the method to obtain the most current values.
        /// </remarks>
        public IDictionary GetConnectionStatistics()
        {
            //Return this back to the caller
            return Connection.RetrieveStatistics();
        }
        #endregion
    }
}