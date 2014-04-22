﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Couchbase.IO;
using Couchbase.IO.Operations.Authentication;

namespace Couchbase.Authentication.SASL
{
    internal class PlainTextMechanism : ISaslMechanism
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private IOStrategy _strategy;

        public PlainTextMechanism(IOStrategy strategy)
        {
            _strategy = strategy;
        }

        public string Username
        {
            get { throw new NotImplementedException(); }
        }

        public string Password
        {
            get { throw new NotImplementedException(); }
        }

        public string MechanismType
        {
            get { return "PLAIN"; }
        }

        public bool Authenticate(string username, string password)
        {
            var authenticated = true;
            foreach (var connection in _strategy.ConnectionPool.Connections)
            {
                var temp = connection;
                Log.Debug(m=>m("Authenticating socket {0}", temp.Identity));

                try
                {
                    var operation = new SaslAuthenticate(MechanismType, username, password);
                    var result = _strategy.Execute(operation, connection);

                    if (!result.Success &&
                        result.Status == ResponseStatus.AuthenticationError)
                    {
                        Log.Debug(m => m("Authentication for socket {0} failed: {1}", temp.Identity, result.Value));
                        authenticated = false;
                        break;
                    }
                    Log.Debug(m => m("Authenticated socket {0} succeeded: {1}", temp.Identity, result.Value));
                }
                catch (Exception e)
                {
                    authenticated = false;
                    Log.Error(e);
                }
            }
            return authenticated;
        }

        public byte[] Authenticate(IO.IConnection connection)
        {
            throw new NotImplementedException();
        }

        public byte[] Continue(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
