using System.Collections.Generic;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	public static class pb_Subdivide
	{
		public static pb_ActionResult Subdivide(this pb_Object pb)
		{
			pb_Face[] subdividedFaces;
			return pb.Subdivide(pb.faces, out subdividedFaces);
		}

		public static pb_ActionResult Subdivide(this pb_Object pb, IList<pb_Face> faces, out pb_Face[] subdividedFaces)
		{
			return pb.Connect(faces, out subdividedFaces);
		}
	}
}
