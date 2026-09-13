namespace ProBuilder.Core
{
	public struct pb_PickerOptions
	{
		public bool depthTest;

		public pb_RectSelectMode rectSelectMode;

		private static readonly pb_PickerOptions k_Default = new pb_PickerOptions
		{
			depthTest = true,
			rectSelectMode = pb_RectSelectMode.Partial
		};

		public static pb_PickerOptions Default
		{
			get
			{
				return k_Default;
			}
		}
	}
}
