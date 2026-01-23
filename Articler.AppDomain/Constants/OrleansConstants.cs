using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Constants
{
    public static class OrleansConstants
    {
        public const string ClusterId = "Silo";
        public const string ServiceId = "Airticler";

        public const double GatewayListRefreshPeriod = 10.0;

        public const string AdoStorageProviderName = "AdoNetStore";

        //public const string UserStateStoreName = "";
        public const string UserStateName = "UserState";
        public const string UserProjectsStateName = "UserProjects";

        public const string ProjectStateName = "ProjectState";
        public const string ProjectTextStateName = "ProjectTextState";
        public const string ProjectDocumentStateName = "ProjectDocumentState";

        public const string ProjectChatHistoryStateName = "ProjectChatHistoryState";
        public const string AgentChatHistoryStateName = "AgentChatHistoryState";
    }
}
