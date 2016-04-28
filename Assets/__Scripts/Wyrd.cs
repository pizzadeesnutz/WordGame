using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wyrd{
	public string str;
	public List<Letter> letters = new List<Letter>();
	public bool found = false;

	public bool visible {
		get{
			if (letters.Count == 0) return false;
			return(letters [0].visible);
		}//end of get
		set{
			foreach (Letter lett in letters) lett.visible = value;
		}//end of set
	}//end of visible

	public Color color{
		get{
			if (letters.Count == 0) return Color.black;
			return(letters [0].color);
		}//end of get
		set{
			foreach (Letter lett in letters) lett.color = value;
		}//end of set
	}//end of color

	public void Add(Letter lett){
		letters.Add (lett);
		str += lett.c.ToString ();
	}//end of Add(Letter lett)
}//end of class