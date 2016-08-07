using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSettings
{
	private string name_ = "UNNAMED_GAME";
	public string name
	{
		get { return name_; }
	}

	private List<LevelSettings> levels_ = new List<LevelSettings>( );
}
