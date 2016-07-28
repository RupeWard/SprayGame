using UnityEngine;
using System.Collections;

/*
public class RandLinearSectionDefnSettingPanel : MonoBehaviour
{
	#region inspector hooks

	public UnityEngine.UI.Text titleText;
	public UnityEngine.UI.Text messageText;

	public FloatSettingPanel floatSettingPanel;
	public Vector2SettingPanel vector2SettingPanel;
	public IntSettingPanel intSettingPanel;

	public UnityEngine.UI.Text numSectionsButtonText;
	public UnityEngine.UI.Text sectionSeparationButtonText;
	public UnityEngine.UI.Text numHoopsPerSectionButtonText;
	public UnityEngine.UI.Text xAngleChangeRangeButtonText;
	public UnityEngine.UI.Text yAngleChangeRangeButtonText;
	public UnityEngine.UI.Text initialRadButtonText;
	public UnityEngine.UI.Text radRangeButtonText;
	public UnityEngine.UI.Text maxRadDButtonText;

	#endregion inspector hooks

	#region inspector data

	#endregion inspector data

	#region actions

	public System.Action< RJWard.Tube.RandLinearSectionDefn > onValueChangedAction;
	
	#endregion actions

	#region private data

	private RJWard.Tube.RandLinearSectionDefn oldSetting_;
	private RJWard.Tube.RandLinearSectionDefn currentSetting_;

	#endregion private data

	private void Awake()
	{
		gameObject.SetActive( false );
	}

	public void Init( string title, RJWard.Tube.RandLinearSectionDefn current, System.Action<RJWard.Tube.RandLinearSectionDefn> changeAction)
	{
		titleText.text = title;
		oldSetting_ = new RJWard.Tube.RandLinearSectionDefn( current);
		currentSetting_ = new RJWard.Tube.RandLinearSectionDefn( current );

		onValueChangedAction = changeAction;
	
		messageText.text = string.Empty;
		messageText.gameObject.SetActive( false );

		SetValue( currentSetting_ );

		gameObject.SetActive( true );
	}

	private void SetValue( RJWard.Tube.RandLinearSectionDefn defn)
	{
		numSectionsButtonText.text = "Num Sections: " + defn.numSections;
		sectionSeparationButtonText.text = "Separation: " + defn.sectionSeparation;
		numHoopsPerSectionButtonText.text = "Hoops per: " + defn.numHoopsPerSection;
		xAngleChangeRangeButtonText.text = "XD: " + defn.xAngleChangeRange.ToString( );
		yAngleChangeRangeButtonText.text = "YD: " + defn.yAngleChangeRange.ToString( );
		initialRadButtonText.text = "Rad0: " + defn.initialRad.ToString( );
		radRangeButtonText.text = "Rad Range: " + defn.radRange.ToString( );
		maxRadDButtonText.text = "Rad Change: " + defn.radChangeRange.ToString( );
	}

	#region handlers

	public void OnNumSectionsButtonClicked()
	{
		intSettingPanel.Init( "Num Sections", currentSetting_.numSections, new int[] { 1, int.MaxValue }, OnNumSectionsChanged );
	}

	public void OnNumSectionsChanged(int i)
	{
		currentSetting_.numSections = i;
		SetValue( currentSetting_ );
		onValueChangedAction( currentSetting_);
	}

	public void OnSectionSeparationButtonClicked( )
	{
		floatSettingPanel.Init( "Section Separation", currentSetting_.sectionSeparation, 
			new Vector2(0.1f, float.MaxValue), OnSectionSeparationChanged );
	}

	public void OnSectionSeparationChanged( float f )
	{
		currentSetting_.sectionSeparation = f;
		SetValue( currentSetting_ );
		onValueChangedAction( currentSetting_ );
	}

	public void OnNumHoopsPerSectionButtonClicked( )
	{
		intSettingPanel.Init( "Num Hoops Per Section", currentSetting_.numHoopsPerSection, new int[] { 1, int.MaxValue }, OnNumHoopsPerSectionChanged);
	}

	public void OnNumHoopsPerSectionChanged( int i )
	{
		currentSetting_.numHoopsPerSection = i;
		SetValue( currentSetting_ );
		onValueChangedAction( currentSetting_ );
	}

	public void OnXAngleChangeRangeButtonClicked( )
	{
		vector2SettingPanel.Init( "X Angle change range", currentSetting_.xAngleChangeRange, new Vector2(-90f, 90f), OnXAngleChangeRangeChanged );
	}

	public void OnXAngleChangeRangeChanged( Vector2 v )
	{
		currentSetting_.xAngleChangeRange = v;
		SetValue( currentSetting_ );
		onValueChangedAction( currentSetting_ );
	}

	public void OnYAngleChangeRangeButtonClicked( )
	{
		vector2SettingPanel.Init( "Y Angle change range", currentSetting_.yAngleChangeRange, new Vector2( -90f, 90f ), OnYAngleChangeRangeChanged );
	}

	public void OnYAngleChangeRangeChanged( Vector2 v )
	{
		currentSetting_.yAngleChangeRange = v;
		SetValue( currentSetting_ );
		onValueChangedAction( currentSetting_ );
	}

	public void OnInitialRadButtonClicked( )
	{
		floatSettingPanel.Init( "Initial Radius", currentSetting_.initialRad,
			currentSetting_.radRange, OnInitialRadChanged);
	}

	public void OnInitialRadChanged( float f )
	{
		currentSetting_.initialRad = f;
		SetValue( currentSetting_ );
		onValueChangedAction( currentSetting_ );
	}

	public void OnRadRangeButtonClicked( )
	{
		vector2SettingPanel.Init( "Rad range", currentSetting_.radRange, new Vector2( 0.5f, float.MaxValue ), OnRadRangeChanged );
	}

	public void OnRadRangeChanged( Vector2 v )
	{
		currentSetting_.radRange = v;
		SetValue( currentSetting_ );
		onValueChangedAction( currentSetting_ );
	}

	public void OnMaxRadDButtonClicked( )
	{
		vector2SettingPanel.Init( "Rad change range", currentSetting_.radChangeRange,
			currentSetting_.radRange, OnRadChangeRangeChanged );
	}

	public void OnRadChangeRangeChanged( Vector2 v )
	{
		currentSetting_.radChangeRange = v;
		SetValue( currentSetting_ );
		onValueChangedAction( currentSetting_ );
	}


	private void DoChangeAction()
	{
		if (onValueChangedAction != null)
		{
			onValueChangedAction( currentSetting_ );
		}
	}

	public void onDoneButtonClicked()
	{
		gameObject.SetActive( false );
	}

	public void onResetButtonClicked( )
	{
		currentSetting_ = oldSetting_;
		SetValue( oldSetting_ );
		if (onValueChangedAction != null)
		{
			onValueChangedAction( oldSetting_ );
		}
	}

	#endregion handlers

	#region factory

	static public RandLinearSectionDefnSettingPanel CreateFromRefab()
	{
		return Resources.Load<GameObject>( "Prefabs/UI/RandLinearSectionDefnSettingPanel" ).GetComponent<RandLinearSectionDefnSettingPanel>( );
	}
	#endregion factory

}
*/
