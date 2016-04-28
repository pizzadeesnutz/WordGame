using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Letter : MonoBehaviour {
	private char _c;
	public TextMesh tMesh;
	public Renderer tRend;

	public bool big = false;

	void Awake(){
		tMesh = GetComponentInChildren<TextMesh> ();
		tRend = tMesh.GetComponent<Renderer> ();
		visible = false;
	}//end of Awake()

	public char c{
		get{
			return _c;
		}//end of get
		set{
			_c = value;
			tMesh.text = _c.ToString ();
		}//end of set
	}//end of c

	public string str{
		get{
			return(_c.ToString ());
		}//end of get
		set{
			c = value [0];
		}//end of set
	}//end of str

	public bool visible{
		get{
			return(tRend.enabled);
		}//end of get
		set{
			tRend.enabled = value;
		}//end of set
	}//end of visible

	public Color color{
		get{
			return (GetComponent<Renderer>().material.color);
		}//end of get
		set{
			GetComponent<Renderer> ().material.color = value;
		}//end of set
	}//end of color

	public Vector3 pos{
		set{
			transform.position = value;
		}//end of set
	}//end of pos
}//end of class