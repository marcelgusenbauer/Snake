using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameBehavior : MonoBehaviour {

    public GameObject Panel;

    public Image ImagePrefab;

    public GameObject GameOverPanel;

    // Start is called before the first frame update
    void Start()
    {
        LoadImages();
        CreateGridLayout();
        CreateANewDragon();
        StartCoroutine("Move");
    }

    private IEnumerator Move()
    {
        var width = Panel.GetComponent<RectTransform>().sizeDelta.x;
        var height = Panel.GetComponent<RectTransform>().sizeDelta.y;

        var PictureSize = width / 20;
        bool IsFinish = true;
        while (IsFinish)
        {
            DirectionOfHead NextDirection = _Dragon.CurrentDirectionOfHead;
            
            if (Input.GetKey(KeyCode.UpArrow) && _Dragon.CurrentDirectionOfHead != DirectionOfHead.Bottom) 
            { NextDirection = DirectionOfHead.Top;}
            
            if (Input.GetKey(KeyCode.DownArrow) && _Dragon.CurrentDirectionOfHead != DirectionOfHead.Top) 
            { NextDirection = DirectionOfHead.Bottom;}
            
            if (Input.GetKey(KeyCode.RightArrow) && _Dragon.CurrentDirectionOfHead != DirectionOfHead.Left) 
            { NextDirection = DirectionOfHead.Right;}
            
            if (Input.GetKey(KeyCode.LeftArrow) && _Dragon.CurrentDirectionOfHead != DirectionOfHead.Right) 
            { NextDirection = DirectionOfHead.Left;}

            var x = 0;
            var y = 0;
            if (NextDirection == DirectionOfHead.Top) { y += 1; }
            if (NextDirection == DirectionOfHead.Bottom) { y += -1; }
            if (NextDirection == DirectionOfHead.Right) { x += 1; }
            if (NextDirection == DirectionOfHead.Left) { x += -1; }

            TurnHeadofDragon(NextDirection);

            System.Tuple<int, int> NextPosition = null;
            var Position_x = _Dragon.Tuples[_Dragon.ListOfParts[0]].Item1;
            var Position_y = _Dragon.Tuples[_Dragon.ListOfParts[0]].Item2;
            NextPosition = new System.Tuple<int, int>(Position_x +x, Position_y +y);

            if(NextPosition.Item1 == 0 || NextPosition.Item1 == 19 || NextPosition.Item2 == 0 || NextPosition.Item2 == 19)
            {
                IsFinish = false;
                GameOver();
            }
            if (_Dragon._DragonParts.ContainsKey(NextPosition))
            {
                IsFinish = false;
                GameOver();
            }
            if (IsFinish)
            {
                List<Image> LastPositions = new List<Image>();
                foreach (var item in _Dragon.ListOfParts)
                {
                    LastPositions.Add(item);
                }
                bool HasTreasure = false;
                if (TreasureDictionary.ContainsKey(NextPosition))
                {
                    HasTreasure = true;
                }

                Dictionary<System.Tuple<int, int>, Image> NewDragonParts = new Dictionary<System.Tuple<int, int>, Image>();
                Dictionary<Image, System.Tuple<int, int>> NewTuples = new Dictionary<Image, System.Tuple<int, int>>();

                Image NewPart = null;
                for (int k = 0; k < 6f; k++)
                {
                    yield return new WaitForSeconds(0);

                    for (int i = 0; i < _Dragon.ListOfParts.Count; i++)
                    {
                        if (i == 0)
                        {

                            _Dragon.ListOfParts[0].transform.position = Vector2.MoveTowards(_Dragon.ListOfParts[0].transform.position,
                                _Positions[NextPosition], 10f);
                            if (k == 0 && HasTreasure)
                            {
                                TreasureDictionary[NextPosition].gameObject.SetActive(false);
                                TreasureDictionary.Clear();
                            }
                            if (k == 0)
                            {
                                NewDragonParts.Add(NextPosition, _Dragon.ListOfParts[0]);
                                NewTuples.Add(_Dragon.ListOfParts[0], NextPosition);
                            }

                        }
                        else
                        {
                            _Dragon.ListOfParts[i].transform.position = Vector2.MoveTowards(_Dragon.ListOfParts[i].transform.position,
                                _Positions[_Dragon.Tuples[LastPositions[i - 1]]], 10f);
                            if (k == 1 && HasTreasure && i == _Dragon.ListOfParts.Count - 1)
                            {
                                NewPart = Instantiate<Image>(ImagePrefab, Panel.transform);
                                NewPart.GetComponent<RectTransform>().sizeDelta = new Vector2(PictureSize, PictureSize);
                                NewPart.sprite = Sprites["Dragonbody"];
                                var PositionOfNewPart = _Dragon.Tuples[_Dragon.ListOfParts.Last()];
                                NewPart.GetComponent<RectTransform>().position = (_Positions[PositionOfNewPart]);
                            }
                            if (k == 0 )
                            {
                                NewDragonParts.Add(_Dragon.Tuples[LastPositions[i - 1]], _Dragon.ListOfParts[i]);
                                NewTuples.Add(_Dragon.ListOfParts[i], _Dragon.Tuples[LastPositions[i - 1]]);
                            }
                        }
                    }
                }
                if (NewPart != null)
                {
                    NewTuples.Add(NewPart, _Dragon.Tuples[LastPositions.Last()]);
                    NewDragonParts.Add(_Dragon.Tuples[LastPositions.Last()], NewPart);
                    _Dragon.ListOfParts.Add(NewPart);
                }

                _Dragon._DragonParts = NewDragonParts;
                _Dragon.Tuples = NewTuples;

                //Add Treasure
                CreateTreasure();
            }
        }
    }

    private void GameOver()
    {
        GameOverPanel.SetActive(true);
    }


    //Add Treasure
    private Dictionary<System.Tuple<int, int>, Image> TreasureDictionary = new Dictionary<System.Tuple<int, int>, Image>();

    private void CreateTreasure()
    {
        if (TreasureDictionary.Count == 0)
        {
            var width = Panel.GetComponent<RectTransform>().sizeDelta.x;
            var height = Panel.GetComponent<RectTransform>().sizeDelta.y;

            var PictureSize = width / 20;
            var finish = true;
            while (finish)
            {
                var randomX = UnityEngine.Random.Range(1, 19);
                var randomY = UnityEngine.Random.Range(1, 19);
                var PositionOfTreasure = new System.Tuple<int, int>(randomX, randomY);
                if (_Dragon._DragonParts.ContainsKey(PositionOfTreasure))
                {

                }
                else
                {
                    var instance = Instantiate<Image>(ImagePrefab, Panel.transform);
                    instance.GetComponent<RectTransform>().sizeDelta = new Vector2(PictureSize, PictureSize);
                    instance.sprite = Sprites["Treasure"];
                    instance.GetComponent<RectTransform>().position = _Positions[PositionOfTreasure];
                    TreasureDictionary.Add(PositionOfTreasure, instance);
                    finish = false;

                }
            }
        }
    }

    private Dragon _Dragon;

    private void CreateANewDragon()
    {
        _Dragon = new Dragon();

        var width = Panel.GetComponent<RectTransform>().sizeDelta.x;
        var height = Panel.GetComponent<RectTransform>().sizeDelta.y;

        var PictureSize = width / 20;

        // Head of Dragon
        var instance = Instantiate<Image>(ImagePrefab, Panel.transform);
        instance.GetComponent<RectTransform>().sizeDelta = new Vector2(PictureSize, PictureSize);
        instance.sprite = Sprites["Dragonhead"];
        var randomX = UnityEngine.Random.Range(5, 15);
        var randomY = UnityEngine.Random.Range(5, 15);
        var PositionOfHead = new System.Tuple<int, int>(randomX, randomY);
        instance.GetComponent<RectTransform>().position = _Positions[PositionOfHead];
        _Dragon._DragonParts.Add(PositionOfHead, instance);
        _Dragon.Tuples.Add(instance, PositionOfHead);
        _Dragon.ListOfParts.Add(instance);

        //Turn head of Dragon
        TurnHeadofDragon(GetRandomDirection());

        //Body of Dragon 1
        instance = Instantiate<Image>(ImagePrefab, Panel.transform);
        instance.GetComponent<RectTransform>().sizeDelta = new Vector2(PictureSize, PictureSize);
        instance.sprite = Sprites["Dragonbody"];
        var PositionFirstBodyPart = GetNextPositionOfBody(PositionOfHead);
        instance.GetComponent<RectTransform>().position = (_Positions[PositionFirstBodyPart]);
        _Dragon._DragonParts.Add(PositionFirstBodyPart, instance);
        _Dragon.Tuples.Add(instance, PositionFirstBodyPart);
        _Dragon.ListOfParts.Add(instance);

        //Body of Dragon 2
        instance = Instantiate<Image>(ImagePrefab, Panel.transform);
        instance.GetComponent<RectTransform>().sizeDelta = new Vector2(PictureSize, PictureSize);
        instance.sprite = Sprites["Dragonbody"];
        var PositionSecondBodyPart = GetNextPositionOfBody(PositionFirstBodyPart);
        instance.GetComponent<RectTransform>().position = (_Positions[PositionSecondBodyPart]);
        _Dragon._DragonParts.Add(PositionSecondBodyPart, instance);
        _Dragon.Tuples.Add(instance, PositionSecondBodyPart);
        _Dragon.ListOfParts.Add(instance);

    }

    private System.Tuple<int, int> GetNextPositionOfBody(System.Tuple<int, int> position)
    {
        var value_x = 0;
        var value_y = 0;
        switch (_Dragon.CurrentDirectionOfHead)
        {

            case DirectionOfHead.Top:
                value_y = -1;
                break;
            case DirectionOfHead.Left:
                value_x = +1;
                break;
            case DirectionOfHead.Bottom:
                value_y = +1;
                break;
            case DirectionOfHead.Right:
                value_x = -1;
                break;
        }
        return new System.Tuple<int, int>(position.Item1 + value_x, position.Item2 + value_y);
    }
    private DirectionOfHead GetRandomDirection()
    {
        switch(UnityEngine.Random.Range(0, 4))
        {
            case 0:
                return DirectionOfHead.Top;
            case 1:
                return DirectionOfHead.Bottom;
            case 2:
                return DirectionOfHead.Left;
            case 3:
                return DirectionOfHead.Right;
            default:
                return DirectionOfHead.Top;

        }
    }

    private void TurnHeadofDragon(DirectionOfHead NextDirection)
    {
        while (true)
        {
            if(NextDirection == _Dragon.CurrentDirectionOfHead) { break; }
            switch (_Dragon.CurrentDirectionOfHead)
            {

                case DirectionOfHead.Top:
                    _Dragon.ListOfParts[0].GetComponent<RectTransform>().Rotate(new Vector3(0, 0, 90));
                    _Dragon.CurrentDirectionOfHead = DirectionOfHead.Left;
                    break;

                case DirectionOfHead.Left:
                    _Dragon.ListOfParts[0].GetComponent<RectTransform>().Rotate(new Vector3(0, 0, 90));
                    _Dragon.CurrentDirectionOfHead = DirectionOfHead.Bottom;
                    break;

                case DirectionOfHead.Bottom:
                    _Dragon.ListOfParts[0].GetComponent<RectTransform>().Rotate(new Vector3(0, 0, 90));
                    _Dragon.CurrentDirectionOfHead = DirectionOfHead.Right;
                    break;

                case DirectionOfHead.Right:
                    _Dragon.ListOfParts[0].GetComponent<RectTransform>().Rotate(new Vector3(0, 0, 90));
                    _Dragon.CurrentDirectionOfHead = DirectionOfHead.Top;
                    break;
            }
        }
    }

    private Dictionary<System.Tuple<int, int>, Vector2> _Positions = new Dictionary<System.Tuple<int, int>, Vector2>();

    private void CreateGridLayout()
    {
        var width = Panel.GetComponent<RectTransform>().sizeDelta.x;
        var height = Panel.GetComponent<RectTransform>().sizeDelta.y;

        var PictureSize = width / 20;
        var StartPosition = new Vector2((width / 20) / 2, (height / 20) / 2);

        for (int y_Axis = 0; y_Axis < 20; y_Axis++)
        {
            var Position_Y = PictureSize * y_Axis;
            for (int x_Axis = 0; x_Axis < 20; x_Axis++)
            {
                var Position_X = PictureSize * x_Axis;
                var instance = Instantiate<Image>(ImagePrefab, Panel.transform);
                if (y_Axis == 0 || y_Axis == 19 || x_Axis == 0 || x_Axis == 19) { instance.color = Color.black; }
                else { instance.color = Color.white; }
                instance.GetComponent<RectTransform>().sizeDelta = new Vector2(PictureSize * 0.95f, PictureSize * 0.95f);
                var PositionOfInstance = new Vector2(StartPosition.x + Position_X, StartPosition.y + Position_Y);
                instance.GetComponent<RectTransform>().position = PositionOfInstance;
                _Positions.Add(new System.Tuple<int, int>(x_Axis, y_Axis), PositionOfInstance);
            }

        }
    }



    private Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

    private void LoadImages()
    {
        var sprites = Resources.LoadAll<Sprite>("Images");
        foreach (var sprite in sprites)
        {
            Sprites.Add(sprite.name, sprite);
        }
    }
}

public class Dragon
{

    public Dictionary<System.Tuple<int, int>, Image> _DragonParts = new Dictionary<System.Tuple<int, int>, Image>();
    public Dictionary<Image, System.Tuple<int, int>> Tuples = new Dictionary<Image, System.Tuple<int, int>>();
    public List<Image> ListOfParts = new List<Image>();

    public DirectionOfHead CurrentDirectionOfHead;

    
    public Dragon()
    {
        CurrentDirectionOfHead = DirectionOfHead.Top;
    }
}


    public enum DirectionOfHead
    {
        Top,
        Bottom,
        Right,
        Left
    }
