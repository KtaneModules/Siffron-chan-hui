using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;



public class siffronScript : MonoBehaviour {

	public KMAudio audio;
	public KMBombInfo bomb;

	public KMSelectable[] buttons;
	private String[] buttonStrings = new String[4];
	private int[] buttonNumbers = new int[4];
	private Boolean[] buttonPressed = new Boolean[4];
	private int[] buttonBValue = new int[4];
	private List <int> sortedButtonBValue = new List <int>();

	public String[] blueWords;
	public int[] blueBrightness;
	private List <String> blueWordsIndex = new List <String>();
	//private int wordIndex = 0;
	private int buttonIndex = 0;

	public String[] Condition1;
	public String[] Condition2;
	public String[] Condition3;
	public String[] Condition4;
	public String[] Condition5;

	public Color[] fontColours;

	private Boolean applyVar = false;
	private int ansCount = 0;
	private int buttonCount = 0;

	private Boolean nowOnStrike = false;

	public string TwitchHelpMessage = "Press top-left button with !{0} press TL or !{0} press 1. You can chain commands. e.g. !{0} press TL BR (numbers in reading order). ";


	//logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool buttonPicked;
	private bool moduleSolved;

	void Awake(){
		moduleId = moduleIdCounter++;
		foreach (KMSelectable button in buttons){
			KMSelectable pressedButton = button;
			if (pressedButton == buttons[0]){
				button.OnInteract += delegate () { buttonPress(pressedButton, 0); return false; };
			}
			if (pressedButton == buttons[1]){
				button.OnInteract += delegate () { buttonPress(pressedButton, 1); return false; };
			}
			if (pressedButton == buttons[2]){
				button.OnInteract += delegate () { buttonPress(pressedButton, 2); return false; };
			}
			if (pressedButton == buttons[3]){
				button.OnInteract += delegate () { buttonPress(pressedButton, 3); return false; };
			}

		}

	}

	void Start(){
		if(!buttonPicked){

			buttonPicked = true;
		}
		DeterminePress();
		PickButton();
		if (applyVar == true){
			SortBValue();
		}
		LoggingCorrect();

	}

	void DeterminePress(){
		//if there are more than three batteries then press the (BV > 180).
		if(bomb.GetBatteryCount() > 3){
			for(int i=0;i<Condition1.Length;i++){
				blueWordsIndex.Add(Condition1[i]);
			}
			Debug.LogFormat("[Siffron #{0}] Apply Condition 1(More than three batteries).", moduleId);
		}
		//Otherwise, if IND or SND is present, then press the (H = 240 || H = 214).
		else if(bomb.IsIndicatorPresent("IND") || bomb.IsIndicatorPresent("SND")){
			for(int i=0;i<Condition2.Length;i++){
				blueWordsIndex.Add(Condition2[i]);
			}
			Debug.LogFormat("[Siffron #{0}] Apply Condition 2(IND or SND is present).", moduleId);
		}
		//Otherwise, if serial number has a vowel, then press the (CV >= 80).
		else if (bomb.GetSerialNumberLetters().Any(x => x == 'A' || x == 'E' || x == 'I' || x == 'O' || x == 'U')){
			for(int i=0;i<Condition3.Length;i++){
				blueWordsIndex.Add(Condition3[i]);
			}
			Debug.LogFormat("[Siffron #{0}] Apply Condition 3(Serial number has a vowel).", moduleId);
		}
		//Otherwise, if serial & parallel is present, then press the (BV % 5 == 0).
		else if (bomb.GetPortCount(Port.Serial) >= 1 && bomb.GetPortCount(Port.Parallel) >= 1){
			for(int i=0;i<Condition4.Length;i++){
				blueWordsIndex.Add(Condition4[i]);
			}
			Debug.LogFormat("[Siffron #{0}] Apply Condition 4(Serial and parallel is present).", moduleId);
		}
		//Otherwise, if there are no batteries, then press the (B > CV).
		else if (bomb.GetBatteryCount() == 0){
			for(int i=0;i<Condition5.Length;i++){
				blueWordsIndex.Add(Condition5[i]);
			}
			Debug.LogFormat("[Siffron #{0}] Apply Condition 5(There are no batteries).", moduleId);
		}
		//Otherwise, press the brightest(in any order).
		else{
			for(int i=0;i<blueWords.Length;i++){
				blueWordsIndex.Add(blueWords[i]);
			}
			Debug.LogFormat("[Siffron #{0}] Apply Condition 6(Previous condition was all false).", moduleId);
			applyVar = true;
		}

	}

	void PickButton(){

		buttonIndex = UnityEngine.Random.Range(0,buttons.Length);
		int RARNG = UnityEngine.Random.Range(0,blueWordsIndex.Count);
		buttons[buttonIndex].GetComponentInChildren<TextMesh>().text = blueWordsIndex[RARNG];
		buttonStrings[buttonIndex] = buttons[buttonIndex].GetComponentInChildren<TextMesh>().text;
		for(int i=0;i<blueWords.Length;i++){
			if(blueWords[i]==blueWordsIndex[RARNG]){
				buttonNumbers[buttonIndex] = i;
			}

		}
		buttonBValue[buttonIndex] = blueBrightness[RARNG];

		for(int i=0;i<buttons.Length;i++){
			if(i != buttonIndex){
				int GRNG = UnityEngine.Random.Range(0,blueWords.Length);
				while(blueWords[GRNG]==buttonStrings[0]||blueWords[GRNG]==buttonStrings[1]||blueWords[GRNG]==buttonStrings[2]||blueWords[GRNG]==buttonStrings[3]){
					GRNG = UnityEngine.Random.Range(0,blueWords.Length);
				}
				buttons[i].GetComponentInChildren<TextMesh>().text = blueWords[GRNG];
				buttonStrings[i] = buttons[i].GetComponentInChildren<TextMesh>().text;
				buttonNumbers[i] = GRNG;
				buttonBValue[i] = blueBrightness[GRNG];
			}

			if(i == 0){
				Debug.LogFormat("[Siffron #{0}] Top-Left Button is {1}.", moduleId, buttons[i].GetComponentInChildren<TextMesh>().text);
			}
			else if(i == 1){
				Debug.LogFormat("[Siffron #{0}] Top-Right Button is {1}.", moduleId, buttons[i].GetComponentInChildren<TextMesh>().text);
			}
			else if(i == 2){
				Debug.LogFormat("[Siffron #{0}] Bottom-Left Button is {1}.", moduleId, buttons[i].GetComponentInChildren<TextMesh>().text);
			}
			else if(i == 3){
				Debug.LogFormat("[Siffron #{0}] Bottom-Right Button is {1}.", moduleId, buttons[i].GetComponentInChildren<TextMesh>().text);
			}

		}

	}

	void SortBValue(){

		for(int i=0;i<buttons.Length;i++){
			sortedButtonBValue.Add(buttonBValue[i]);
		}
		sortedButtonBValue.Sort();
		Debug.LogFormat("[Siffron #{0}] Brightest colour has {1}% of brightness.", moduleId, sortedButtonBValue[3]);

		blueWordsIndex.Clear();
		for(int i=0;i<blueWords.Length;i++){
			if(blueBrightness[i] == sortedButtonBValue[3]){
					blueWordsIndex.Add(blueWords[i]);
			}

		}

	}

	void LoggingCorrect(){

		String correctColours = "";
		String correctButtons = "";

		for(int i=0;i<blueWords.Length;i++){
			if(blueWordsIndex.Contains(blueWords[i])){
				if(correctColours != ""){
					correctColours += ", ";
				}
				correctColours += blueWords[i];
			}

		}

		for(int i=0;i<blueWords.Length;i++){
			if(blueWordsIndex.Contains(blueWords[i])&&(blueWords[i]==buttonStrings[0]||blueWords[i]==buttonStrings[1]||blueWords[i]==buttonStrings[2]||blueWords[i]==buttonStrings[3])){
				if(correctButtons != ""){
					correctButtons += ", ";
				}
				correctButtons += blueWords[i];
			}

		}

		Debug.LogFormat("[Siffron #{0}] Colour that matches with rule is {1}.", moduleId, correctColours);
		Debug.LogFormat("[Siffron #{0}] Button that matches with rule is {1}.", moduleId, correctButtons);

	}

	void buttonPress(KMSelectable button, int indv){

		if(moduleSolved||nowOnStrike||buttonPressed[indv]){
			return;
		}

		button.AddInteractionPunch();
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

		Debug.LogFormat("[Siffron #{0}] You pressed {1}.", moduleId, button.GetComponentInChildren<TextMesh>().text);

		if (blueWordsIndex.Contains(button.GetComponentInChildren<TextMesh>().text)){
			button.GetComponentInChildren<TextMesh>().color = fontColours[buttonNumbers[indv]+2];
			buttonPressed[indv] = true;
		}
		else{
			GetComponent<KMBombModule>().HandleStrike();
			Debug.LogFormat("[Siffron #{0}] Strike! You pressed wrong button.", moduleId);
			for(int i=0;i<4;i++){
				buttons[i].GetComponentInChildren<TextMesh>().color = fontColours[0];
				buttonPressed[i] = false;
			}
			blueWordsIndex.Clear();
			sortedButtonBValue.Clear();
			applyVar = false;
			StartCoroutine(Strike(indv));
			return;
		}

		for(int i=0;i<4;i++){
			if(blueWordsIndex.Contains(buttonStrings[i])){
				ansCount++;
			}
			if(buttonPressed[i] == true){
				buttonCount++;
			}
		}

		if(ansCount == buttonCount){
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
			Debug.LogFormat("[Siffron #{0}] All correct button pressed, Module solved.", moduleId);
			GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
			StartCoroutine(Solved());
		}

		ansCount = 0;
		buttonCount = 0;

	}

	IEnumerator Solved(){

		for(int i=0;i<4;i++){
			buttons[i].GetComponentInChildren<TextMesh>().color = fontColours[0];
		}

		yield return new WaitForSeconds(0.1f);

		for(int i=0;i<4;i++){
			buttons[i].GetComponentInChildren<TextMesh>().color = fontColours[buttonNumbers[i]+2];
			yield return new WaitForSeconds(0.1f);
		}

		int flash = 0;

		while(flash < 3){
				for(int i=0;i<4;i++){
					buttons[i].GetComponentInChildren<TextMesh>().color = fontColours[0];
				}
				yield return new WaitForSeconds(0.1f);
				for(int i=0;i<4;i++){
					buttons[i].GetComponentInChildren<TextMesh>().color = fontColours[buttonNumbers[i]+2];
				}
				yield return new WaitForSeconds(0.1f);

				flash++;
		}
	}

	IEnumerator Strike(int indv){

		nowOnStrike = true;

		int flash = 0;
		while(flash < 5){
				buttons[indv].GetComponentInChildren<TextMesh>().color = fontColours[1];
				yield return new WaitForSeconds(0.1f);
				buttons[indv].GetComponentInChildren<TextMesh>().color = fontColours[0];
				yield return new WaitForSeconds(0.1f);

				flash++;
		}

		nowOnStrike = false;
		Debug.LogFormat("[Siffron #{0}] Resetting module...", moduleId);
		Start();
	}

	public List <KMSelectable> ProcessTwitchCommand(string command){

		string[] cutInBlank = command.Split(new char[] {' '});
		List <KMSelectable> whichToPress = new List <KMSelectable>();

		if (cutInBlank[0] == "press")
		{
			for(int i=1;i<cutInBlank.Length;i++){
				if(cutInBlank[i] == "TL" || cutInBlank[i] == "1"){
					whichToPress.Add(buttons[0]);
				}
				if(cutInBlank[i] == "TR" || cutInBlank[i] == "2"){
					whichToPress.Add(buttons[1]);
				}
				if(cutInBlank[i] == "BL" || cutInBlank[i] == "3"){
					whichToPress.Add(buttons[2]);
				}
				if(cutInBlank[i] == "BR" || cutInBlank[i] == "4"){
					whichToPress.Add(buttons[3]);
				}
			}
			return whichToPress;
    }


    return null;
	}

}
