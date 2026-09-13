using System.Collections.Generic;
using System.Text;

namespace Photon.Chat
{
	public class ChatChannel
	{
		public readonly string Name;

		public readonly List<string> Senders = new List<string>();

		public readonly List<object> Messages = new List<object>();

		public int MessageLimit;

		private Dictionary<object, object> properties;

		public readonly HashSet<string> Subscribers = new HashSet<string>();

		public bool IsPrivate { get; protected internal set; }

		public int MessageCount
		{
			get
			{
				return Messages.Count;
			}
		}

		public int LastMsgId { get; protected set; }

		public bool PublishSubscribers { get; protected set; }

		public int MaxSubscribers { get; protected set; }

		public ChatChannel(string name)
		{
			Name = name;
		}

		public void Add(string sender, object message, int msgId)
		{
			Senders.Add(sender);
			Messages.Add(message);
			LastMsgId = msgId;
			TruncateMessages();
		}

		public void Add(string[] senders, object[] messages, int lastMsgId)
		{
			Senders.AddRange(senders);
			Messages.AddRange(messages);
			LastMsgId = lastMsgId;
			TruncateMessages();
		}

		public void TruncateMessages()
		{
			if (MessageLimit > 0 && Messages.Count > MessageLimit)
			{
				int count = Messages.Count - MessageLimit;
				Senders.RemoveRange(0, count);
				Messages.RemoveRange(0, count);
			}
		}

		public void ClearMessages()
		{
			Senders.Clear();
			Messages.Clear();
		}

		public string ToStringMessages()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < Messages.Count; i++)
			{
				stringBuilder.AppendLine(string.Format("{0}: {1}", Senders[i], Messages[i]));
			}
			return stringBuilder.ToString();
		}

		internal void ReadProperties(Dictionary<object, object> newProperties)
		{
			if (newProperties == null || newProperties.Count <= 0)
			{
				return;
			}
			if (properties == null)
			{
				properties = new Dictionary<object, object>(newProperties.Count);
			}
			foreach (object key in newProperties.Keys)
			{
				if (newProperties[key] == null)
				{
					if (properties.ContainsKey(key))
					{
						properties.Remove(key);
					}
				}
				else
				{
					properties[key] = newProperties[key];
				}
			}
			if (properties.ContainsKey((byte)254))
			{
				object obj = properties[(byte)254];
				PublishSubscribers = obj != null && (bool)obj;
			}
			if (properties.ContainsKey(byte.MaxValue))
			{
				object obj = properties[byte.MaxValue];
				if (obj == null)
				{
					MaxSubscribers = 0;
				}
				else
				{
					MaxSubscribers = (int)obj;
				}
			}
		}

		internal bool TryAddSubscriber(string user)
		{
			return !string.IsNullOrEmpty(user) && !Subscribers.Contains(user) && Subscribers.Add(user);
		}

		internal void AddSubscribers(string[] users)
		{
			if (users != null)
			{
				for (int i = 0; i < users.Length; i++)
				{
					TryAddSubscriber(users[i]);
				}
			}
		}
	}
}
