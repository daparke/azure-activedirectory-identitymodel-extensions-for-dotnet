﻿//-----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System;
using System.Diagnostics.Tracing;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace Microsoft.IdentityModel.Logging.Tests
{
    public class LoggerTests
    {

        [Fact(DisplayName = "LoggerTests : LogMessageAndThrowException")]
        public void LogMessageAndThrowException()
        {
            SampleListener listener = new SampleListener();
            IdentityModelEventSource.LogLevel = EventLevel.Verbose;             // since null parameters exceptions are logged at Verbose level
            listener.EnableEvents(IdentityModelEventSource.Logger, EventLevel.Verbose);

            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                SecurityToken token;

                // This should log an error and throw null argument exception.
                handler.ValidateToken(null, null, out token);
            }
            catch (Exception ex)
            {
                Assert.Equal(ex.GetType(), typeof(ArgumentNullException));
                Assert.Contains("IDX10000: The parameter 'System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler: securityToken' cannot be a 'null' or an empty string.", listener.TraceBuffer);
            }
        }

        [Fact(DisplayName = "LogggerTests : LogMessage")]
        public void LogMessage()
        {
            SampleListener listener = new SampleListener();
            IdentityModelEventSource.LogLevel = EventLevel.Verbose;
            listener.EnableEvents(IdentityModelEventSource.Logger, EventLevel.Verbose);

            TokenValidationParameters validationParameters = new TokenValidationParameters()
            {
                ValidateAudience = false
            };

            // This should log a warning about not validating the audience
            Validators.ValidateAudience(null, null, validationParameters);
            Assert.Contains("IDX10233: ", listener.TraceBuffer);
        }

        [Fact(DisplayName = "LoggerTests : TestLogLevel")]
        public void TestLogLevel()
        {
            SampleListener listener = new SampleListener();
            IdentityModelEventSource.LogLevel = EventLevel.Informational;
            listener.EnableEvents(IdentityModelEventSource.Logger, EventLevel.Verbose);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            handler.CreateToken();

            // This is Informational level message. Should be there in the trace buffer since default log level is informational.
            Assert.Contains("IDX10722: ", listener.TraceBuffer);
            // This is Verbose level message. Should not be there in the trace buffer.
            Assert.DoesNotContain("IDX10721: ", listener.TraceBuffer);

            // Setting log level to verbose so that all messages are logged.
            IdentityModelEventSource.LogLevel = EventLevel.Verbose;
            handler.CreateToken();
            Assert.Contains("IDX10722: ", listener.TraceBuffer);
            Assert.Contains("IDX10721: ", listener.TraceBuffer);

        }
    }

    class SampleListener : EventListener
    {
        public string TraceBuffer { get; set; }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            TraceBuffer += eventData.Payload[0] + "\n";
        }
    }
}