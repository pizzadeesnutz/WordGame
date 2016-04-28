using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WordLevel{ //doesn't extend MonoBehavior
	public int levelNum;
	public int longWordIndex;
	public string word;
	public Dictionary<char,int> charDict;
	public List<string> subWords;

	//this function makes a charcter dictionary of the provided string
	static public Dictionary<char,int> MakeCharDict(string w){
		Dictionary<char,int> dict = new Dictionary<char,int> ();
		char c;

		for (int i = 0; i < w.Length; i++) {
			c = w [i];
			if (dict.ContainsKey (c)) dict [c]++;
			else dict.Add (c, 1);
		}//end of for loop

		return dict;
	}//end of MakeCharDict(string w)

	//this function checks to see if a word can be spelled with available letters
	public static bool CheckWordInLevel(string str, WordLevel level){
		Dictionary <char,int> counts = new Dictionary <char,int> ();

		for (int i = 0; i < str.Length; i++) {
			char c = str [i];

			//if the level contains char c
			if (level.charDict.ContainsKey (c)) {
				//if the key already isn't in counts
				if (!counts.ContainsKey (c)) counts.Add (c, 1);
				//otherwise add 1 to the current value
				else counts [c]++;
				//if there aren't enough of the character
				if (counts [c] > level.charDict [c]) return false;
			}//end of if

			//or if it doesn't contain char c
			else return false;
		}//end of for loop

		return true;
	}//end of CheckWordInLevel(string str, WordLevel level)
}//end of class