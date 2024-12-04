using System;
using System.Threading.Tasks;

namespace Utilities.ConsoleCommands
{
    public class CommandAction
    {
        private readonly string _key;
        private readonly string _actionName;
        private readonly Func<Task<int>> _action;

        public CommandAction(string key, string actionName, Func<Task<int>> action)
        {
            this._key = key;
            this._actionName = actionName;
            this._action = action;
        }

        public string Key
        {
            get
            {
                return this._key;
            }
        }

        public string ActionName
        {
            get
            {
                return this._actionName;
            }
        }

        public Func<Task<int>> Action
        {
            get
            {
                return this._action;
            }
        }
    }
}