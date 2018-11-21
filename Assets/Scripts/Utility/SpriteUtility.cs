using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class SpriteUtility
{
	static public bool FillSpriteMesh(this Sprite _sprite, ref List<Vector3> _vertices, ref List<Vector2> _uv, ref List<int> _indices)
	{
		for(int indx = 0; indx < _sprite.vertices.Length; ++indx)
			_vertices.Add(_sprite.vertices[indx]);

		for(int indx = 0; indx < _sprite.uv.Length; ++indx)
			_uv.Add(_sprite.uv[indx]);

		for(int indx = 0; indx < _sprite.triangles.Length; ++indx)
			_indices.Add(_sprite.triangles[indx]);

		return true;
	}

	static public bool FillSpriteMesh(this Sprite _sprite, out UnityEngine.Mesh _mesh)
	{
		List<Vector3> vertices = new List<Vector3>(_sprite.vertices.Length);
		List<Vector2> uvs = new List<Vector2>(_sprite.uv.Length);
		List<int> indices = new List<int>(_sprite.triangles.Length);

		for(int indx = 0; indx < _sprite.vertices.Length; ++indx)
			vertices.Add(_sprite.vertices[indx]);

		for(int indx = 0; indx < _sprite.uv.Length; ++indx)
			uvs.Add(_sprite.uv[indx]);

		for(int indx = 0; indx < _sprite.triangles.Length; ++indx)
			indices.Add(_sprite.triangles[indx]);

		_mesh = new Mesh();
		_mesh.SetVertices(vertices);
		_mesh.SetUVs(0, uvs);
		_mesh.SetTriangles(indices, 0);

		_mesh.UploadMeshData(true);

		return true;
	}
}
