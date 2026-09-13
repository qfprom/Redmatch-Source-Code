using System;
using UnityEngine;

namespace ProBuilder.Core
{
	public static class pb_Constant
	{
		public const string PRODUCT_NAME = "ProBuilder";

		internal const HideFlags k_EditorHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.NotEditable;

		internal const float k_MaxPointDistanceFromControl = 20f;

		internal const char DEGREE_SYMBOL = '°';

		internal const char CMD_SUPER = '⌘';

		internal const char CMD_SHIFT = '⇧';

		internal const char CMD_OPTION = '⌥';

		internal const char CMD_ALT = '⎇';

		internal const char CMD_DELETE = '⌫';

		internal const string pbDefaultEditLevel = "pbDefaultEditLevel";

		internal const string pbDefaultSelectionMode = "pbDefaultSelectionMode";

		internal const string pbHandleAlignment = "pbHandleAlignment";

		internal const string pbVertexColorTool = "pbVertexColorTool";

		internal const string pbToolbarLocation = "pbToolbarLocation";

		internal const string pbDefaultEntity = "pbDefaultEntity";

		internal const string pbExtrudeMethod = "pbExtrudeMethod";

		internal const string pbDefaultStaticFlags = "pbDefaultStaticFlags";

		internal const string pbSelectedFaceColor = "pbDefaultFaceColor";

		internal const string pbWireframeColor = "pbDefaultEdgeColor";

		internal const string pbUnselectedEdgeColor = "pbUnselectedEdgeColor";

		internal const string pbSelectedEdgeColor = "pbSelectedEdgeColor";

		internal const string pbSelectedVertexColor = "pbDefaultSelectedVertexColor";

		internal const string pbUnselectedVertexColor = "pbDefaultVertexColor";

		internal const string pbPreselectionColor = "pbPreselectionColor";

		internal const string pbDefaultOpenInDockableWindow = "pbDefaultOpenInDockableWindow";

		internal const string pbEditorPrefVersion = "pbEditorPrefVersion";

		internal const string pbEditorShortcutsVersion = "pbEditorShortcutsVersion";

		internal const string pbDefaultCollider = "pbDefaultCollider";

		internal const string pbForceConvex = "pbForceConvex";

		internal const string pbVertexColorPrefs = "pbVertexColorPrefs";

		internal const string pbShowEditorNotifications = "pbShowEditorNotifications";

		[Obsolete]
		internal const string pbDragCheckLimit = "pbDragCheckLimit";

		internal const string pbForceVertexPivot = "pbForceVertexPivot";

		internal const string pbForceGridPivot = "pbForceGridPivot";

		internal const string pbManifoldEdgeExtrusion = "pbManifoldEdgeExtrusion";

		internal const string pbPerimeterEdgeBridgeOnly = "pbPerimeterEdgeBridgeOnly";

		internal const string pbPBOSelectionOnly = "pbPBOSelectionOnly";

		internal const string pbCloseShapeWindow = "pbCloseShapeWindow";

		internal const string pbUVEditorFloating = "pbUVEditorFloating";

		internal const string pbUVMaterialPreview = "pbUVMaterialPreview";

		[Obsolete]
		internal const string pbShowSceneToolbar = "pbShowSceneToolbar";

		internal const string pbNormalizeUVsOnPlanarProjection = "pbNormalizeUVsOnPlanarProjection";

		internal const string pbStripProBuilderOnBuild = "pbStripProBuilderOnBuild";

		internal const string pbDisableAutoUV2Generation = "pbDisableAutoUV2Generation";

		internal const string pbShowSceneInfo = "pbShowSceneInfo";

		internal const string pbEnableBackfaceSelection = "pbEnableBackfaceSelection";

		internal const string pbVertexPaletteDockable = "pbVertexPaletteDockable";

		internal const string pbExtrudeAsGroup = "pbExtrudeAsGroup";

		internal const string pbUniqueModeShortcuts = "pbUniqueModeShortcuts";

		internal const string pbMaterialEditorFloating = "pbMaterialEditorFloating";

		internal const string pbShapeWindowFloating = "pbShapeWindowFloating";

		internal const string pbIconGUI = "pbIconGUI";

		internal const string pbShiftOnlyTooltips = "pbShiftOnlyTooltips";

		[Obsolete]
		internal const string pbDrawAxisLines = "pbDrawAxisLines";

		internal const string pbCollapseVertexToFirst = "pbCollapseVertexToFirst";

		internal const string pbMeshesAreAssets = "pbMeshesAreAssets";

		internal const string pbElementSelectIsHamFisted = "pbElementSelectIsHamFisted";

		internal const string pbFillHoleSelectsEntirePath = "pbFillHoleSelectsEntirePath";

		internal const string pbDetachToNewObject = "pbDetachToNewObject";

		[Obsolete("Use pb_MeshImporter::quads")]
		internal const string pbPreserveFaces = "pbPreserveFaces";

		[Obsolete("Use pbRectSelectMode")]
		internal const string pbDragSelectWholeElement = "pbDragSelectWholeElement";

		internal const string pbRectSelectMode = "pbRectSelectMode";

		internal const string pbDragSelectMode = "pbDragSelectMode";

		internal const string pbShadowCastingMode = "pbShadowCastingMode";

		internal const string pbEnableExperimental = "pbEnableExperimental";

		internal const string pbCheckForProBuilderUpdates = "pbCheckForProBuilderUpdates";

		internal const string pbManageLightmappingStaticFlag = "pbManageLightmappingStaticFlag";

		internal const string pbShowMissingLightmapUvWarning = "pb_Lightmapping::showMissingLightmapUvWarning";

		internal const string pbSelectedFaceDither = "pbSelectedFaceDither";

		internal const string pbUseUnityColors = "pbUseUnityColors";

		internal const string pbVertexHandleSize = "pbVertexHandleSize";

		internal const string pbUVGridSnapValue = "pbUVGridSnapValue";

		internal const string pbUVWeldDistance = "pbUVWeldDistance";

		internal const string pbLineHandleSize = "pbLineHandleSize";

		internal const string pbWireframeSize = "pbWireframeSize";

		internal const string pbWeldDistance = "pbWeldDistance";

		internal const string pbExtrudeDistance = "pbExtrudeDistance";

		internal const string pbBevelAmount = "pbBevelAmount";

		internal const string pbEdgeSubdivisions = "pbEdgeSubdivisions";

		internal const string pbDefaultShortcuts = "pbDefaultShortcuts";

		internal const string pbDefaultMaterial = "pbDefaultMaterial";

		internal const string pbCurrentMaterialPalette = "pbCurrentMaterialPalette";

		internal const string pbGrowSelectionUsingAngle = "pbGrowSelectionUsingAngle";

		internal const string pbGrowSelectionAngle = "pbGrowSelectionAngle";

		internal const string pbGrowSelectionAngleIterative = "pbGrowSelectionAngleIterative";

		internal const string pbShowDetail = "pbShowDetail";

		internal const string pbShowOccluder = "pbShowOccluder";

		internal const string pbShowMover = "pbShowMover";

		internal const string pbShowCollider = "pbShowCollider";

		internal const string pbShowTrigger = "pbShowTrigger";

		internal const string pbShowNoDraw = "pbShowNoDraw";

		internal static readonly Rect RectZero = new Rect(0f, 0f, 0f, 0f);

		internal static Color ProBuilderBlue = new Color(0f, 0.682f, 0.937f, 1f);

		internal static Color ProBuilderLightGray = new Color(0.35f, 0.35f, 0.35f, 0.4f);

		internal static Color ProBuilderDarkGray = new Color(0.1f, 0.1f, 0.1f, 0.3f);

		public const int MENU_ABOUT = 0;

		public const int MENU_EDITOR = 100;

		public const int MENU_SELECTION = 200;

		public const int MENU_GEOMETRY = 200;

		public const int MENU_ACTIONS = 300;

		public const int MENU_MATERIAL_COLORS = 400;

		public const int MENU_VERTEX_COLORS = 400;

		public const int MENU_REPAIR = 600;

		public const int MENU_MISC = 600;

		public const int MENU_EXPORT = 800;

		[Obsolete("Use pb_Material.Default")]
		public static Material DefaultMaterial
		{
			get
			{
				return pb_Material.DefaultMaterial;
			}
		}
	}
}
