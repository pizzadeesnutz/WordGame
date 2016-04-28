using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WordList : MonoBehaviour {
	public static WordList S;

	public TextAsset wordListText;
	public int numToParseBeforeYield = 10000;
	public int wordLengthMin = 3;
	public int wordLengthMax = 7;

	public bool __________________;

	public int currLine = 0;
	public int totalLines;
	public int longWordCount;
	public int wordCount;
	//private variables
	private string[] lines;
	private List<string> longWords;
	private List<string> words;

	void Awake(){
		S = this;
	}//end of Awake()

	//unlike Start() Init() will wait till called to begin
	public void Init() {
		//split the lines on "line feeds"
		lines = wordListText.text.Split('\n');
		totalLines = lines.Length;
		StartCoroutine (ParseLines ());
	}//end of Init()

	public IEnumerator ParseLines(){
		string word;
		longWords = new List<string>();
		words = new List<string>();

		for (currLine = 0; currLine < totalLines; currLine++) {
			word = lines [currLine];

			//is the word a long word? if so store in longwords
			if (word.Length == wordLengthMax) {
				longWords.Add (word);
			}//end of if

			//is it between the min and max size? if so add it to valid word list
			if (word.Length >= wordLengthMin && word.Length <= wordLengthMax) {
				words.Add (word);
			}//end of if

			//is the currrent line a multipe of our yield point?
			if (currLine % numToParseBeforeYield == 0) {
				longWordCount = longWords.Count;
				wordCount = words.Count;
				yield return null;
			}//end of if
		}//end of for loop
		gameObject.SendMessage("WordListParseComplete");
	}//end of ParseLines()

	//************** accessor functions ***************//
	public List<string> GetWords(){
		return(words);
	}//end of GetWords()

	public string GetWord(int ndx){
		return(words [ndx]);
	}//end of GetWord(int ndx)

	public List<string> GetLongWords(){
		return (longWords);
	}//end of GetLongWords()

	public string GetLongWord(int ndx){
		return (longWords [ndx]);
	}//end of GetLongWord(int ndx)
	//*********** end of accessor functions ***********//
}//end of class