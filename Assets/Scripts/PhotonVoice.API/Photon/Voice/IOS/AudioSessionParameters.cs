namespace Photon.Voice.IOS
{
	public struct AudioSessionParameters
	{
		public AudioSessionCategory Category;

		public AudioSessionMode Mode;

		public AudioSessionCategoryOption[] CategoryOptions;

		public int CategotyOptionsToInt()
		{
			int num = 0;
			if (CategoryOptions != null)
			{
				for (int i = 0; i < CategoryOptions.Length; i++)
				{
					num |= (int)CategoryOptions[i];
				}
			}
			return num;
		}

		public override string ToString()
		{
			string text = "[";
			if (CategoryOptions != null)
			{
				for (int i = 0; i < CategoryOptions.Length; i++)
				{
					text += CategoryOptions[i];
					if (i != CategoryOptions.Length - 1)
					{
						text += ", ";
					}
				}
			}
			text += "]";
			return string.Format("category = {0}, mode = {1}, options = {2}", Category, Mode, text);
		}
	}
}
