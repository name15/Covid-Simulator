using UnityEngine;
using Newtonsoft.Json;

public class InfectEarth : MonoBehaviour
{
	const int countryCount = 177;

	public Material EarthMaterial;
	public TextAsset jsonCountries;

	public string[] countries;

	private float[] infectionStatus = new float[countryCount];

	public void InfectCountry(int countryId, float value) {
		infectionStatus[countryId] = value;
		EarthMaterial.SetFloatArray("_InfectionStatus", infectionStatus);
	}

	private void Start() {
		countries = JsonConvert.DeserializeObject<CountriesScheme>(jsonCountries.text).array;
	}
}
