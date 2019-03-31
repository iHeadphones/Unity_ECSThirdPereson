using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GInputName
{
	NONE,
    KEYBOARD,
	PS4,
	XBOXONE,
	SWITCH,
}

public enum GButton
{
	TOP,
	RIGHT,
	BOTTOM,
	LEFT,
	L1,
	L2,
	L3,
	R1,
	R2,
	R3,
}

public enum GAxis
{
	LEFTHORIZONTAL,
	LEFTVERTICAL,
	RIGHTHORIZONTAL,
	RIGHTVERTICAL,
	DHORIZONTAL,
	DVERTICAL
}

public class GInput
{
	static float previousDHorizontal;
	static float previousDVertical;

	public void Start()
	{
		
	}

	public static bool GetButtonDown(GButton button)
	{
		return Input.GetKeyDown(RemapKeyCode(button));
	}

	public static bool GetButton(GButton button)
	{
		return Input.GetKey(RemapKeyCode(button));
	}

	private static KeyCode RemapKeyCode(GButton button)
	{
		var name = GetControllerName();

        //Keyboard
        if (name == GInputName.KEYBOARD)
        {   
            if (button == GButton.TOP) return KeyCode.LeftShift;
			else if (button == GButton.RIGHT) return KeyCode.None;
			else if (button == GButton.BOTTOM) return KeyCode.Space;
			else if (button == GButton.LEFT) return KeyCode.Mouse0;
			else if (button == GButton.L1) return KeyCode.JoystickButton4;
			else if (button == GButton.L2) return KeyCode.Mouse1;
			else if (button == GButton.L3) return KeyCode.LeftControl;
			else if (button == GButton.R1) return KeyCode.JoystickButton5;
			else if (button == GButton.R2) return KeyCode.JoystickButton7;
			else if (button == GButton.R3) return KeyCode.JoystickButton11;
        }

		//PS4
		if (name == GInputName.PS4)
		{
			if (button == GButton.TOP) return KeyCode.JoystickButton3;
			else if (button == GButton.RIGHT) return KeyCode.JoystickButton2;
			else if (button == GButton.BOTTOM) return KeyCode.JoystickButton1;
			else if (button == GButton.LEFT) return KeyCode.JoystickButton0;
			else if (button == GButton.L1) return KeyCode.JoystickButton4;
			else if (button == GButton.L2) return KeyCode.JoystickButton6;
			else if (button == GButton.L3) return KeyCode.JoystickButton10;
			else if (button == GButton.R1) return KeyCode.JoystickButton5;
			else if (button == GButton.R2) return KeyCode.JoystickButton7;
			else if (button == GButton.R3) return KeyCode.JoystickButton11;
		}

		//XBOX ONE
		if (name == GInputName.XBOXONE)
		{
			if (button == GButton.TOP) return KeyCode.JoystickButton3;
			else if (button == GButton.RIGHT) return KeyCode.JoystickButton1;
			else if (button == GButton.BOTTOM) return KeyCode.JoystickButton0;
			else if (button == GButton.LEFT) return KeyCode.JoystickButton2;
			else if (button == GButton.L1) return KeyCode.JoystickButton4;
			else if (button == GButton.L2) return KeyCode.JoystickButton5;
			else if (button == GButton.L3) return KeyCode.JoystickButton10;
			else if (button == GButton.R1) return KeyCode.JoystickButton5;
			else if (button == GButton.R2) return KeyCode.JoystickButton7;
			else if (button == GButton.R3) return KeyCode.JoystickButton11;
		}

		return KeyCode.None;
	}

	public static float GetAxisRaw(GAxis axis)
	{
		return RemapAxis(axis,false);
	}

	public static float GetAxisDown(GAxis axis)
	{
		return RemapAxis(axis,true);
	}

	private static float RemapAxis(GAxis axis, bool down)
	{
		var name = GetControllerName();

		string stick = "joystick 1 axis 0";

		if (name == GInputName.NONE)
			return 0;
		
		if (axis == GAxis.LEFTHORIZONTAL) return Input.GetAxisRaw("LeftHorizontal"+ name.ToString());
		else if (axis == GAxis.LEFTVERTICAL) return Input.GetAxisRaw("LeftVertical"+ name.ToString());
		else if (axis == GAxis.RIGHTHORIZONTAL) return Input.GetAxisRaw("RightHorizontal"+ name.ToString());
		else if (axis == GAxis.RIGHTVERTICAL) return Input.GetAxisRaw("RightVertical"+ name.ToString());
		else if (axis == GAxis.DHORIZONTAL && (!down || down && previousDHorizontal == 0)) return Input.GetAxisRaw("DHorizontal" + name.ToString());
		else if (axis == GAxis.DVERTICAL && (!down || down && previousDVertical == 0)) return Input.GetAxisRaw("DVertical" + name.ToString());

		return 0;
	}

	public static void Update()
	{
		var name = GetControllerName();
		if (name == GInputName.NONE)
			return;

		previousDHorizontal = Input.GetAxisRaw("DHorizontal" + name.ToString()) ;
		previousDVertical = Input.GetAxisRaw("DVertical" + name.ToString());
	}
	
	public static GInputName GetControllerName()
	{
		string[] names = Input.GetJoystickNames();

        if (names == null || names.Length == 0)
            return GInputName.KEYBOARD;
		
		

		switch (names[0].Length)
		{
			case 19: return GInputName.PS4; 
			case 47: return GInputName.PS4; 
			case 33: return GInputName.XBOXONE;
			case 22: return GInputName.SWITCH;
			default: return GInputName.NONE;
		}
	}
}