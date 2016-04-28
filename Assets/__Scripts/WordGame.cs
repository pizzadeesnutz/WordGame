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
	public Rect wordArea = new Rect(24, 19, 48, 28);
	public float letterSize = 1.5f;
	public bool showAllWyrds = true;
	public float bigLetterSize = 4f;
	public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
	public Color bigColorSelected = Color.white;
	public Vector3 bigLetterCenter = new Vector3(0, -16, 0);
	public List <float> scoreFontSizes = new List<float> {24, 36, 36, 1};
	public Vector3 scoreMidPoint = new Vector3(1,1,0);
	public float scoreComboDelay = 0.5f;
	public Color[] wyrdPalette;

	public bool _________________;

	public GameMode mode = GameMode.preGame;
	public WordLevel  currLevel;
	public List<Wyrd> wyrds;
	public List<Letter> bigLetters;
	public List<Letter> bigLettersActive;
	public string testWord;
	private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	void Awake(){
		S = this;
	}//end of Awake()

	void Start () {
		mode = GameMode.loading;
		WordList.S.Init ();
	}//end of Start()

	void Update(){
		Letter lett;
		char c;

		switch (mode){
		case GameMode.inLevel:
			foreach (char cIt in Input.inputString) {
				c = System.Char.ToUpperInvariant (cIt);

				if (upperCase.Contains (c)) {
					lett = FindNextLetterByChar (c);
					if (lett != null) {
						testWord += c.ToString ();
						bigLettersActive.Add (lett);
						bigLetters.Remove (lett);
						lett.color = bigColorSelected;
						ArrangeBigLetters ();
					}//end of nested if
				}//end of if

				//if the user hit backspace
				if (c == '\b') {
					if (bigLettersActive.Count == 0) return;
					if (testWord.Length > 1) testWord = testWord.Substring (0, testWord.Length - 1);
					else testWord = "";

					lett = bigLettersActive [bigLettersActive.Count - 1];
					bigLettersActive.Remove (lett);
					bigLetters.Add (lett);
					lett.color = bigColorDim;
					ArrangeBigLetters ();
				}//end of if

				//if the user hits enter or return check to see if the word is right
				if (c == '\n' || c == '\r') {
					StartCoroutine(CheckWord ());
				}//end of if

				//if the users enters a space then shuffle the bigLetters
				if (c == ' ') {
					bigLetters = ShuffleLetters (bigLetters);
					ArrangeBigLetters ();
				}//end of if
			}//end of foreach
			break;
		}//end of switch
	}//end of Update()
		
	void Score(Wyrd wyrd, int combo){
		Vector3 pt = wyrd.letters [0].transform.position;
		List<Vector3> pts = new List <Vector3> ();

		pt = Camera.main.WorldToViewportPoint (pt);
		pt.z = 0;

		pts.Add (pt);

		pts.Add (scoreMidPoint);

		pts.Add (Scoreboard.S.transform.position);

		int value = wyrd.letters.Count * combo;
		FloatingScore fs = Scoreboard.S.CreateFloatingScore (value, pts);

		fs.timeDuration = 2f;
		fs.fontSizes = scoreFontSizes;

		fs.easingCurve = Easing.InOut + Easing.InOut;

		string txt = wyrd.letters.Count.ToString ();
		if (combo > 1) txt += " x " + combo;

		fs.GetComponent<GUIText> ().text = txt;
	}//end of Score(Wyrd wyrd, int combo)

	Letter FindNextLetterByChar(char c){
		foreach (Letter l in bigLetters) {
			if (l.c == c) return l;
		}//end of foreach
		return null;
	}//end of FindNextLetterByChar(char c)

	public IEnumerator CheckWord(){
		string subword;
		bool foundTestWord = false;

		List<int> containedWords = new List<int> ();
		for (int i = 0; i < currLevel.subWords.Count; i++) {
			if (wyrds [i].found) continue;
			subword = currLevel.subWords [i];

			if (string.Equals (testWord, subword)) {
				HighlightWyrd (i);
				Score (wyrds [i], 1);
				foundTestWord = true;
			}//end of if
			else if (testWord.Contains (subword)) {
				containedWords.Add (i);
			}//end of else if
		}//end of for loop

		if (foundTestWord) {
			int numContained = containedWords.Count;
			int ndx;
			for (int i = 0; i < containedWords.Count; i++) {
				yield return (new WaitForSeconds (scoreComboDelay));
				ndx = numContained - i - 1;
				HighlightWyrd (containedWords [ndx]);
				Score (wyrds [containedWords [ndx]], i + 2);
			}//end of for loop
		}//end of if

		ClearBigLettersActive ();
	}//end of CheckWord()

	void HighlightWyrd(int ndx){
		wyrds [ndx].found = true;
		wyrds [ndx].color = (wyrds [ndx].color + Color.white) / 2f;
		wyrds [ndx].visible = true;
	}//end of HighlightWyrd(int ndx)

	void ClearBigLettersActive(){
		testWord = "";
		foreach (Letter l in bigLettersActive) {
			bigLetters.Add (l);
			l.color = bigColorDim;
		}//end of foreach
		bigLettersActive.Clear();
		ArrangeBigLetters ();
	}//end of ClearBigLettersActive()

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

	void Layout() {
		wyrds = new List<Wyrd>();

		GameObject go;
		Letter lett;
		string word;
		Vector3 pos;
		float left = 0;
		float columnWidth = 3;
		char c;
		Color col;
		Wyrd wyrd;

		int numRows = Mathf.RoundToInt(wordArea.height/letterSize);

		for (int i=0; i<currLevel.subWords.Count; i++) {
			wyrd = new Wyrd();
			word = currLevel.subWords[i];

			columnWidth = Mathf.Max( columnWidth, word.Length );

			for (int j=0; j<word.Length; j++) {
				c = word[j]; 
				go = Instantiate(prefabLetter) as GameObject;
				lett = go.GetComponent<Letter>();
				lett.c = c; 
				pos = new Vector3(wordArea.x+left+j*letterSize, wordArea.y, 0);
				lett.timeStart = Time.time + i*0.05f;
				pos.y -= (i%numRows)*letterSize;
				lett.position = pos+Vector3.up*(20+i%numRows);
				lett.pos = pos;
				lett.timeStart = Time.time + i*0.05f;
				go.transform.localScale = Vector3.one*letterSize;
				wyrd.Add(lett);
			}//end of for j loop

			if (showAllWyrds) wyrd.visible = true;

			wyrd.color = wyrdPalette[word.Length-WordList.S.wordLengthMin];
			wyrds.Add(wyrd);

			if (i%numRows == numRows-1) left += (columnWidth+0.5f)*letterSize;
		}//end of for i loop

		bigLetters = new List<Letter>();
		bigLettersActive = new List<Letter>();

		for (int i=0; i<currLevel.word.Length; i++) {
			c = currLevel.word[i];
			go = Instantiate(prefabLetter) as GameObject;
			lett = go.GetComponent<Letter>();
			lett.c = c;
			go.transform.localScale = Vector3.one*bigLetterSize;
			pos = new Vector3( 0, -100, 0 );
			lett.position = pos;
			lett.timeStart = Time.time + currLevel.subWords.Count*0.05f;
			lett.easingCurve = Easing.Sin+"-0.18"; 
			col = bigColorDim;
			lett.color = col;
			lett.visible = true; 
			lett.big = true;
			bigLetters.Add(lett);
		}//end of for loop

		bigLetters = ShuffleLetters(bigLetters);
		ArrangeBigLetters();
		mode = GameMode.inLevel;
	}//end of Layout()

	List<Letter> ShuffleLetters(List<Letter> letts){
		int ndx;
		List<Letter> newL = new List<Letter> ();
		while (letts.Count > 0) {
			ndx = Random.Range (0, letts.Count);
			newL.Add (letts [ndx]);
			letts.RemoveAt (ndx);
		}//end of while loop
		return newL;
	}//end of ShuffleLetters(List<Letter> letts)

	void ArrangeBigLetters(){
		float halfWidth = ((float)bigLetters.Count) / 2f - .5f;
		Vector3 pos;
		for (int i = 0; i < bigLetters.Count; i++) {
			pos = bigLetterCenter;
			pos.x += (i - halfWidth) * bigLetterSize;
			pos.z = -3;
			bigLetters [i].pos = pos;
		}//end of for loop
		halfWidth = ((float) bigLettersActive.Count)/2f - .5f;
		for (int i = 0; i < bigLettersActive.Count; i++) {
			pos = bigLetterCenter;
			pos.x += (i - halfWidth) * bigLetterSize;
			pos.y += bigLetterSize * 1.25f;
			pos.z = -3;
			bigLettersActive [i].pos = pos;
		}//end of for loop
	}//end of ArrangeBigLetters()
}//end of class