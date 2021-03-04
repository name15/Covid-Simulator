using UnityEngine;
using UnityEngine.SceneManagement;

using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace DecisionTreeScheme {
	[XmlRoot(ElementName = "country")]
	public class Country {
		[XmlAttribute(AttributeName = "id")]
		public int Id;
		[XmlAttribute(AttributeName = "level")]
		public float Level;
	}

	[XmlRoot(ElementName = "infect")]
	public class Infect {
		[XmlElement(ElementName = "country")]
		public List<Country> Country;
	}

	[XmlRoot(ElementName = "answer")]
	public class Answer {
		[XmlAttribute(AttributeName = "action")]
		public int Action;
		[XmlText]
		public string Text;
	}

	[XmlRoot(ElementName = "answers")]
	public class Answers {
		[XmlElement(ElementName = "answer")]
		public List<Answer> Answer;
	}

	[XmlRoot(ElementName = "action")]
	public class Action {
		[XmlElement(ElementName = "title")]
		public string Title;
		[XmlElement(ElementName = "infect")]
		public Infect Infect;
		[XmlElement(ElementName = "question")]
		public string Question;
		[XmlElement(ElementName = "answers")]
		public Answers Answers;
	}

	[XmlRoot(ElementName = "actions")]
	public class Actions {
		[XmlElement(ElementName = "action")]
		public List<Action> Action;
	}
}

public class DecisionTree : MonoBehaviour
{
	public TextAsset xmlActions;

	public InfectEarth infect;
	public OrbitEarth orbit;

	public TMPro.TextMeshProUGUI TitleText;
	public TMPro.TextMeshProUGUI QuestionText;
	public Transform ButtonContainer;
	public GameObject ButtonPrefab;

	private DecisionTreeScheme.Actions actions;

	private void Start() {
		string xml = xmlActions.text;
		var serializer = new XmlSerializer(typeof(DecisionTreeScheme.Actions));
		
		using (TextReader reader = new StringReader(xml)) {
			actions = (DecisionTreeScheme.Actions)serializer.Deserialize(reader);
		}

		Debug.Log(actions.Action[0].Infect.Country[0].Id);

		DisplayAction(0);
	}
	public void DisplayAction(int actionID) {
		Debug.Log(actionID);
		DecisionTreeScheme.Action action = actions.Action[actionID];

		// Infect Countries
		foreach (DecisionTreeScheme.Country country in action.Infect.Country) {
			infect.InfectCountry(country.Id, country.Level);
		}

		// Rotate camera
		switch (actionID) {
			case 0:
				orbit.SetView(new Vector3(1.75f, 0.6f, 7.5f));
				orbit.SetView(new Vector3(1.75f, 0.6f, 7.5f));
				break;
			case 9:
				orbit.SetView(new Vector3(0.22f, 0.725f, 6f));
				break;
			case 18:
				orbit.SetView(new Vector3(-1.7f, 0.675f, 7.5f));
				break;
		}

		// Display action title, question & button text
		TitleText.text = action.Title;
		QuestionText.text = action.Question;

		foreach (Transform button in ButtonContainer) Destroy(button.gameObject); // Destroy all previous buttons

		if (action.Answers.Answer.Count > 0) {
			foreach (DecisionTreeScheme.Answer answer in action.Answers.Answer) {
				GameObject button = Instantiate(ButtonPrefab, ButtonContainer); // Create new Button in Button Comtainer

				TMPro.TextMeshProUGUI ButtonText = button.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
				int resultingAction = answer.Action;

				ButtonText.text = answer.Text;
				button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate () {  // Call this method recursively on click
					DisplayAction(resultingAction);
				});
			} 
		} else {
			GameObject button = Instantiate(ButtonPrefab, ButtonContainer);
			button.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Обратно към главното меню";
			button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate () {  // Call this method recursively on click
				SceneManager.LoadScene("Menu");
			});
		}
	}
}