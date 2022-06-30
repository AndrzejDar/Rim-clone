using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TropicsUtils
{
    public static class TropicsUtilsClass
    {
        public const int sortingOrderDefault = 5000;



        // Creates a Text Mesh in the World and constantly updates it
        public static FunctionUpdater CreateWorldTextUpdater(Func<string> GetTextFunc, Vector3 localPosition, Transform parent = null)
        {
            TextMesh textMesh = CreateWorldText(GetTextFunc(), parent, localPosition);
            return FunctionUpdater.Create(() => {
                textMesh.text = GetTextFunc();
                return false;
            }, "WorldTextUpdater");
        }




        // Create Text in the World
        public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 100, Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = sortingOrderDefault)
        {
            if (color == null) color = Color.white;
            return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
        }

        // Create Text in the World
        public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.characterSize = 0.01f;
            //textMesh.fontSize = 128;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;
        }

        // Create a Text Popup in the World, no parent
        public static void CreateWorldTextPopup(string text, Vector3 localPosition)
        {
            CreateWorldTextPopup(null, text, localPosition+new Vector3(-0.4f,0.6f), 100, Color.white, localPosition + new Vector3(-0.4f, 1), 5f);
        }

        // Create a Text Popup in the World
        public static void CreateWorldTextPopup(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, Vector3 finalPopupPosition, float popupTime)
        {
            TextMesh textMesh = CreateWorldText(parent, text, localPosition, fontSize, color, TextAnchor.LowerLeft, TextAlignment.Left, sortingOrderDefault);
            Transform transform = textMesh.transform;
            Vector3 moveAmount = (finalPopupPosition - localPosition) / popupTime;
            FunctionUpdater.Create(delegate () {
                transform.position += moveAmount * Time.deltaTime;
                popupTime -= Time.deltaTime;
                if (popupTime <= 0f)
                {
                    UnityEngine.Object.Destroy(transform.gameObject);
                    return true;
                }
                else
                {
                    return false;
                }
            }, "WorldTextPopup");
        }











    }





}


