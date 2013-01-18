using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EIBA.Interop.Falcon;
using Knx.Infrastructure.EventArguments;
using Knx.Infrastructure;
using Knx.Infrastructure.DataTypes;

namespace Knx.Router
{
    public sealed class RouterActor : IDisposable
    {
        IConnection _connection;
        AutoDisposeConnectionObject _auto;
        public RouterActor(bool useDefault = true)
        {
            ConnectionMode connectionMode = ConnectionMode.ConnectionModeRemoteConnectionless;
            IConnectionManager connectionManager = new ConnectionManager();
            FalconConnection falconConnection = default(FalconConnection);
							if (useDefault)
								falconConnection = connectionManager.GetDefaultConnection();
							else
								falconConnection = connectionManager.GetConnection("", 1);

            _connection = ConnectionObjectFactory.CreateUnlicensedConnectionObject(null);
            _auto = new AutoDisposeConnectionObject(_connection);
            _connection.Mode = connectionMode;

            if (falconConnection.guidEdi == Guid.Empty)
                throw new Exception("Operation was aborted");
						DeviceOpenError err = _connection.Open2("{" + falconConnection.guidEdi.ToString() + "}",
									falconConnection.Parameters);
            if (err != DeviceOpenError.DeviceOpenErrorNoError)
            {
                throw new InvalidOperationException(string.Format("Could not open connection: {0}", err.ToString()));
            }

            beginConfirmedGroupDataChannel();
        }

        public void Write(EnmxAddress address, int data)
        {
            bool lessThan7Bits = false;
            var bytes = BitConverter.GetBytes(data);
            if (data < 128)
                lessThan7Bits = true;
            _confirmedGroupData.Write(address.Address, EIBA.Interop.Falcon.Priority.PriorityLow, 6, lessThan7Bits, bytes);
        }

        public event EventHandler<ReceivedGroupTelegramEventArgs> ReceivedGroupTelegram;

        private void OnReceivedGroupTelegram(int address, int routing, Priority priority, object data, GroupTelegramTypes type)
        {
            GroupTelegram gt = new GroupTelegram(address, data, DateTime.Now, routing, (int)priority, type);
            if (ReceivedGroupTelegram != null)
                ReceivedGroupTelegram(this, new ReceivedGroupTelegramEventArgs(gt));
        }

        #region ConfirmedGroupData
        IGroupDataTransfer _groupDataTransfer;
        GroupData _confirmedGroupData;
        private void beginConfirmedGroupDataChannel()
        {
            _confirmedGroupData = new GroupData();
            _groupDataTransfer = (IGroupDataTransfer)_confirmedGroupData;

            _confirmedGroupData.GroupDataConfirmationRead += OnGroupDataConfirmationRead;
            _confirmedGroupData.GroupDataConfirmationResponse += OnGroupDataConfirmationResponse;
            _confirmedGroupData.GroupDataConfirmationWrite += OnGroupDataConfirmationWrite;

            _confirmedGroupData.GroupDataIndicationRead += OnGroupDataIndicationRead;
            _confirmedGroupData.GroupDataIndicationResponse += OnGroupDataIndicationResponse;
            _confirmedGroupData.GroupDataIndicationWrite += OnGroupDataIndicationWrite;

            _confirmedGroupData.Connection = _connection;
        }

        private void endConfirmedGroupDataChannel()
        {
            _confirmedGroupData.GroupDataConfirmationRead -= OnGroupDataConfirmationRead;
            _confirmedGroupData.GroupDataConfirmationResponse -= OnGroupDataConfirmationResponse;
            _confirmedGroupData.GroupDataConfirmationWrite -= OnGroupDataConfirmationWrite;

            _confirmedGroupData.GroupDataIndicationRead -= OnGroupDataIndicationRead;
            _confirmedGroupData.GroupDataIndicationResponse -= OnGroupDataIndicationResponse;
            _confirmedGroupData.GroupDataIndicationWrite -= OnGroupDataIndicationWrite;
        }

        void OnGroupDataConfirmationRead(int GroupAddress, int RoutingCnt, Priority Prio, bool Error, object Data)
        {
            if (!Error)
                OnReceivedGroupTelegram(GroupAddress, RoutingCnt, Prio, Data, GroupTelegramTypes.ConfirmationRead);
        }

        void OnGroupDataConfirmationResponse(int GroupAddress, int RoutingCnt, Priority Prio, bool Error, object Data)
        {
            if (!Error)
                OnReceivedGroupTelegram(GroupAddress, RoutingCnt, Prio, Data, GroupTelegramTypes.ConfirmationResponse);
        }

        void OnGroupDataConfirmationWrite(int GroupAddress, int RoutingCnt, Priority Prio, bool Error, object Data)
        {
            if (!Error)
                OnReceivedGroupTelegram(GroupAddress, RoutingCnt, Prio, Data, GroupTelegramTypes.ConfirmationWrite);
        }

        void OnGroupDataIndicationRead(int GroupAddress, int RoutingCnt, Priority Prio, object Data)
        {
            OnReceivedGroupTelegram(GroupAddress, RoutingCnt, Prio, Data, GroupTelegramTypes.IndicationRead);
        }

        void OnGroupDataIndicationResponse(int GroupAddress, int RoutingCnt, Priority Prio, object Data)
        {
            OnReceivedGroupTelegram(GroupAddress, RoutingCnt, Prio, Data, GroupTelegramTypes.IndicationResponse);
        }

        void OnGroupDataIndicationWrite(int GroupAddress, int RoutingCnt, Priority Prio, object Data)
        {
            OnReceivedGroupTelegram(GroupAddress, RoutingCnt, Prio, Data, GroupTelegramTypes.IndicationWrite);
        }
        #endregion

        public void Dispose()
        {
            endConfirmedGroupDataChannel();
            _auto.Dispose();
        }
    }
}
