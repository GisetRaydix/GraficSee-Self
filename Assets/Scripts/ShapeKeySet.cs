using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeKeySet : MonoBehaviour
{
    public KeyCode GetKeyCodeFromChar(char charKey)
    {
        switch (charKey)
        {
            case 'a':
                return KeyCode.A;
            case 'b':
                return KeyCode.B;
            case 'c':
                return KeyCode.C;
            case 'd':
                return KeyCode.D;
            case 'e':
                return KeyCode.E;
            case 'f':
                return KeyCode.F;
            case 'g':
                return KeyCode.G;
            case 'h':
                return KeyCode.H;
            case 'i':
                return KeyCode.I;
            case 'j':
                return KeyCode.J;
            case 'k':
                return KeyCode.K;
            case 'l':
                return KeyCode.L;
            case 'm':
                return KeyCode.M;
            case 'n':
                return KeyCode.N;
            case 'o':
                return KeyCode.O;
            case 'p':
                return KeyCode.P;
            case 'q':
                return KeyCode.Q;
            case 'r':
                return KeyCode.R;
            case 's':
                return KeyCode.S;
            case 't':
                return KeyCode.T;
            case 'u':
                return KeyCode.U;
            case 'v':
                return KeyCode.V;
            case 'w':
                return KeyCode.W;
            case 'x':
                return KeyCode.X;
            case 'y':
                return KeyCode.Y;
            case 'z':
                return KeyCode.Z;
            case '0':
                return KeyCode.Alpha0;
            case '1':
                return KeyCode.Alpha1;
            case '2':
                return KeyCode.Alpha2;
            case '3':
                return KeyCode.Alpha3;
            case '4':
                return KeyCode.Alpha4;
            case '5':
                return KeyCode.Alpha5;
            case '6':
                return KeyCode.Alpha6;
            case '7':
                return KeyCode.Alpha7;
            case '8':
                return KeyCode.Alpha8;
            case '9':
                return KeyCode.Alpha9;
            case '-':
                return KeyCode.Minus;
            case '^':
                return KeyCode.Caret;
            case '\\':
                return KeyCode.Backslash;
            case '@':
                return KeyCode.At;
            case '[':
                return KeyCode.LeftBracket;
            case ';':
                return KeyCode.Semicolon;
            case ':':
                return KeyCode.Colon;
            case ']':
                return KeyCode.RightBracket;
            case ',':
                return KeyCode.Comma;
            case '.':
                return KeyCode.Period;
            case '/':
                return KeyCode.Slash;
            case '_':
                return KeyCode.Underscore;
            case '\b':
                return KeyCode.Backspace;
            case '\r':
                return KeyCode.Return;
            case ' ':
                return KeyCode.Space;
            default:
                return KeyCode.None; //ñ¢ímÇÃï∂éöÇÃèÍçáÇÕKeyCode.NoneÇï‘Ç∑
        }
    }
}
