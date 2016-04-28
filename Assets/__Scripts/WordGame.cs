using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum GameMode{
	preGame,
	loading,
	makeLevel,
	levelPrep,
	inLevel
}//end of GameMode enum

public class WordGame : MonoBehaviour {
	static public WordGame S;

	public GameObject prefabLetter;
	public Rect wordArea = new Rect(-24, 19, 48, 28);
	public float letterSize = 1.5f;
	public bool showAllWyrds = true;
	public float bigLetterSize = 4f;

	public bool _________________;

	public GameMode mode = GameMode.preGame;
	public WordLevel  currLevel;
	public List<Wyrd> wyrds;

	void Awake(){
		S = this;
	}//end of Awake()

	void Start () {
		mode = GameMode.loading;
		WordList.S.Init ();
	}//end of Start()

	public void WordListParseComplete(){
		mode = GameMode.makeLevel;
		currLevel = MakeWordLevel ();
	}//end of WordListParseComplete()

	public WordLevel MakeWordLevel(int levelNum = -1){
		WordLevel level = new WordLevel ();
		if (levelNum == -1) level.longWordIndex = Random.Range (0, WordList.S.longWordCount);
		else {

		}//end of else
		level.levelNum = levelNum;
		level.word = WordList.S.GetLongWord (level.longWordIndex);
		level.charDict = WordLevel.MakeCharDict (level.word);

		//can the word be spelled by the letters in level.charDict
		StartCoroutine(FindSubWordsCoroutine(level));

		//because level is returned before the coroutine is finished SubWordSearchComplete()
		//will be called when the coroutine is finished
		return level;
	}//end of MakeWordLevel(int levelNum = -1)

	//a coroutine to find words that can be spelled in this level
	public IEnumerator FindSubWordsCoroutine(WordLevel level){
		level.subWords = new List<string> ();
		string str;

		List<string> words = WordList.S.GetWords ();

		for (int i = 0; i < WordList.S.wordCount; i++) {
			str = words [i];

			//can the word be spelled?
			if(WordLevel.CheckWordInLevel(str,level)) level.subWords.Add(str);
			if (i % WordList.S.numToParseBeforeYield == 0) yield return null;	
		}//end of for loop

		//sort the list of words alphabetically
		level.subWords.Sort();

		//now sort by length of word
		level.subWords = SortWordsByLength(level.subWords).ToList();

		//now the coroutine is complete so call the function to let the system know
		SubWordSearchComplete();
	}//end of FindSubWordsCoroutine(WordLevel level)

	public static IEnumerable<string> SortWordsByLength(IEnumerable<string> e){
		//this uses LINQ syntax beyond the scope of this course
		var sorted = from s in e
		             orderby s.Length ascending
		             select s;
		return sorted;
	}//end of SortWordsByLength(IEnumerable<string> e)

	public void SubWordSearchComplete(){
		mode = GameMode.levelPrep;
		Layout ();
	}//end of SubWordSearchComplete()

	void Layout(){
		wyrds = new List<Wyrd> ();

		GameObject go;
		Letter lett;
		string word;
		Vector3 pos;
		float left = 0;
		float columnWidth = 3;
		char c;
		Color col;
		Wyrd wyrd;

		//how many rows of letters will fit on the screen?
		int numRows = Mathf.RoundToInt(wordArea.height/letterSize);

		//make a Wyrd from each level.subWord
		for (int i = 0; i < currLevel.subWords.Count; i++) {
			wyrd = new Wyrd ();
			word = currLevel.subWords [i];

			//expand the column if the word is bigger
			columnWidth = Mathf.Max(columnWidth, word.Length);

			//instatiate a prefab for each letter
			for (int j = 0; j < word.Length; j++) {
				c = word [j];
				go = Instantiate(prefabLetter) as GameObject;
				lett = go.GetComponent<Letter> ();
				lett.c = c;
				pos = new Vector3 (wordArea.x + left + j * letterSize, wordArea.y, 0);
				pos.y = (i % numRows) * letterSize;
				lett.pos = pos;

				go.transform.localScale = Vector3.one * letterSize;
				wyrd.Add (lett);
			}//end of for j loop

			if (showAllWyrds) wyrd.visible = true;

			wyrds.Add (wyrd);

			if (i % numRows == numRows - 1) left += (columnWidth + .5f)* letterSize;
		}//end of for i loop
	}//end of Layout()
}//end of class