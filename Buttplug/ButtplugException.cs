using System;

namespace Buttplug
{
    public class ButtplugException : Exception
    {
        public ButtplugException(string aMessage, Exception aInner = null)
            : base(aMessage, aInner)
        {
        }

        public static ButtplugException FromError(ServerMessage.Types.Error aMsg)
        {
            var err_str = aMsg.Message;
            switch (aMsg.ErrorType)
            {
                case ServerMessage.Types.ButtplugErrorType.ButtplugConnectorError:
                    return new ButtplugConnectorException(err_str);
                case ServerMessage.Types.ButtplugErrorType.ButtplugPingError:
                    return new ButtplugPingException(err_str);
                case ServerMessage.Types.ButtplugErrorType.ButtplugMessageError:
                    return new ButtplugMessageException(err_str);
                case ServerMessage.Types.ButtplugErrorType.ButtplugUnknownError:
                    return new ButtplugUnknownException(err_str);
                case ServerMessage.Types.ButtplugErrorType.ButtplugHandshakeError:
                    return new ButtplugHandshakeException(err_str);
                case ServerMessage.Types.ButtplugErrorType.ButtplugDeviceError:
                    return new ButtplugDeviceException(err_str);
            }
            return new ButtplugUnknownException($"Unknown error type: {aMsg.ErrorType} | Message: {aMsg.Message}");
        }
    }
}
