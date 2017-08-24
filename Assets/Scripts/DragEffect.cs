using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HorizontalDrag
{
    LEFT,
    RIGHT,
    NONE
}

public enum VerticalDrag
{
    UP,
    DOWN,
    NONE
}

/// <summary>
/// When dragging a piece (either player or NPC) the piece will tilt in the direction it is being dragged
/// </summary>
public class DragEffect
{
    HorizontalDrag h_drag = HorizontalDrag.NONE;
    VerticalDrag v_drag = VerticalDrag.NONE;
    Vector3 previousPosition;
    float startXLerp = 0;
    float startYLerp = 0;
    float xAxis = 0;
    float yAxis = 0;
    float startXTime;
    float startYTime;
    bool resetXLerp = false;
    bool resetYLerp = false;
    float rollMult;
    float pitchMult;
    float sensitivity;
    float gravity;

    public DragEffect(Vector3 position, float roll, float pitch, float s=3, float g=3)
    {
        previousPosition = position;
        rollMult = roll;
        pitchMult = pitch;
        sensitivity = s;
        gravity = g;
    }

    public Quaternion Tilt(Vector3 position)
    {
        DragHorizontal(position);
        DragVertical(position);
        previousPosition = position;
        xAxis = GetDragAxis("Horizontal");
        yAxis = GetDragAxis("Vertical");
        return Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);
    }

    public Quaternion ResetTilt()
    {
        //Quaternion q = new Quaternion();
        if (resetXLerp)
        {
            xAxis = LerpX(startXLerp, 0, gravity);
            if (xAxis == 0) { resetXLerp = false; }
            //q = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);
        }

        if (resetYLerp)
        {
            yAxis = LerpY(startYLerp, 0, gravity);
            if (yAxis == 0) { resetYLerp = false; }
            //q = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);
        }
        return Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);
    }

    public void ResetTiltValues()
    {
        StartLerpTime();      
        startXLerp = xAxis;
        startYLerp = yAxis;
        resetXLerp = true;
        resetYLerp = true;
        h_drag = HorizontalDrag.NONE;
        v_drag = VerticalDrag.NONE;
    }

    // Start of the tilt Lerp
    public void StartLerpTime()
    {
        startXTime = Time.time;
        startYTime = Time.time;
    }

    float GetDragAxis(string axis)
    {
        float u = 0;
        if (axis == "Horizontal")
        {
            if (h_drag == HorizontalDrag.RIGHT)
            {
                u = LerpX(startXLerp, 1, sensitivity);
            }
            else if (h_drag == HorizontalDrag.LEFT)
            {
                u = LerpX(startXLerp, -1, sensitivity);
            }
        }
        else if (axis == "Vertical")
        {
            if (v_drag == VerticalDrag.UP)
            {
                u = LerpY(startYLerp, 1, sensitivity);
            }
            else if (v_drag == VerticalDrag.DOWN)
            {
                u = LerpY(startYLerp, -1, sensitivity);
            }
        }
        return u;
        
    }

    // Interpolate from start -> end
    float LerpX(float start, float end, float value)
    {
        float u = (Time.time - startXTime) * value;  //0->1   
        if (u > 1) { u = 1; }
        u = (1 - u) * start + u * end;
        return u;
    }

    float LerpY(float start, float end, float value)
    {
        float u = (Time.time - startYTime) * value;  //0->1      
        if (u > 1) { u = 1; }
        u = (1 - u) * start + u * end;
        return u;
    }

    void DragHorizontal(Vector3 position)
    {
        Vector3 vec = position - previousPosition;
        if (vec.x > 0)
        {
            if (h_drag != HorizontalDrag.RIGHT)
            {
                startXTime = Time.time;
                startXLerp = xAxis;
            }
            h_drag = HorizontalDrag.RIGHT;
        }
        else if (vec.x < 0)
        {
            if (h_drag != HorizontalDrag.LEFT)
            {
                startXTime = Time.time;
                startXLerp = xAxis;
            }
            h_drag = HorizontalDrag.LEFT;
        }
    }

    void DragVertical(Vector3 position)
    {
        Vector3 vec = position - previousPosition;
        if (vec.y > 0)
        {
            if (v_drag != VerticalDrag.UP)
            {
                startYTime = Time.time;
                startYLerp = yAxis;
            }
            v_drag = VerticalDrag.UP;
        }
        else if (vec.y < 0)
        {
            if (v_drag != VerticalDrag.DOWN)
            {
                startYTime = Time.time;
                startYLerp = yAxis;
            }
            v_drag = VerticalDrag.DOWN;
        }
    }
}
