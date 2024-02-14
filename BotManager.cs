using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    [SerializeField] GameObject Bot1;
    [SerializeField] GameObject Bot2;
    [SerializeField] GameObject Bot3;
    [SerializeField] GameObject Bot4;
    [SerializeField] GameObject Bot5;
    [SerializeField] GameObject Bot6;
    [SerializeField] GameObject Board1;
    [SerializeField] GameObject Board2;
    [SerializeField] GameObject Board3;
    [SerializeField] GameObject[] BotList = new GameObject[6];
    [SerializeField] bool IsOn;

    public void RunGameBoard1(){
        Board1.GetComponent<BoardManager>().OnGame();
        Board2.GetComponent<BoardManager>().OnGame();
        Board3.GetComponent<BoardManager>().OnGame();
        Board1.GetComponent<BoardManager>().SetBoardType1();
        Board2.GetComponent<BoardManager>().SetBoardType1();
        Board3.GetComponent<BoardManager>().SetBoardType1();
        StartCoroutine(RunGameBoardsCort());
    }

    public void RunTraining(){
        StartCoroutine(RunTrain());
    }

    IEnumerator RunTrain(){
        while (IsOn == true){
            RandomizeBots();
            yield return new WaitForSeconds(0.1f);
            Debug.Log("Hi");
            GameObject Holder = null;
            RunGameBoard1();
            Holder = Bot1.GetComponent<ChessBot8>().GetUsedBoard();
            if ((Holder.GetComponent<BoardManager>().GetWinner() == true && Bot1.GetComponent<ChessBot8>().GetSide() == 1) || (Holder.GetComponent<BoardManager>().GetWinner() == false && Bot1.GetComponent<ChessBot8>().GetSide() == 2)){
                Bot1.GetComponent<ChessBot8>().RandomizeEvalValues();
            }
            Holder = Bot2.GetComponent<ChessBot8>().GetUsedBoard();
            if ((Holder.GetComponent<BoardManager>().GetWinner() == true && Bot2.GetComponent<ChessBot8>().GetSide() == 1) || (Holder.GetComponent<BoardManager>().GetWinner() == false && Bot2.GetComponent<ChessBot8>().GetSide() == 2)){
                Bot2.GetComponent<ChessBot8>().RandomizeEvalValues();
            }
            Holder = Bot3.GetComponent<ChessBot8>().GetUsedBoard();
            if ((Holder.GetComponent<BoardManager>().GetWinner() == true && Bot3.GetComponent<ChessBot8>().GetSide() == 1) || (Holder.GetComponent<BoardManager>().GetWinner() == false && Bot3.GetComponent<ChessBot8>().GetSide() == 2)){
                Bot3.GetComponent<ChessBot8>().RandomizeEvalValues();
            }
            Holder = Bot4.GetComponent<ChessBot8>().GetUsedBoard();
            if ((Holder.GetComponent<BoardManager>().GetWinner() == true && Bot4.GetComponent<ChessBot8>().GetSide() == 1) || (Holder.GetComponent<BoardManager>().GetWinner() == false && Bot4.GetComponent<ChessBot8>().GetSide() == 2)){
                Bot4.GetComponent<ChessBot8>().RandomizeEvalValues();
            }
            Holder = Bot5.GetComponent<ChessBot8>().GetUsedBoard();
            if ((Holder.GetComponent<BoardManager>().GetWinner() == true && Bot5.GetComponent<ChessBot8>().GetSide() == 1) || (Holder.GetComponent<BoardManager>().GetWinner() == false && Bot5.GetComponent<ChessBot8>().GetSide() == 2)){
                Bot5.GetComponent<ChessBot8>().RandomizeEvalValues();
            }
            Holder = Bot6.GetComponent<ChessBot8>().GetUsedBoard();
            if ((Holder.GetComponent<BoardManager>().GetWinner() == true && Bot6.GetComponent<ChessBot8>().GetSide() == 1) || (Holder.GetComponent<BoardManager>().GetWinner() == false && Bot6.GetComponent<ChessBot8>().GetSide() == 2)){
                Bot6.GetComponent<ChessBot8>().RandomizeEvalValues();
            }
        }
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator RunGameBoardsCort(){

        while (Board1.GetComponent<BoardManager>().GetGameState() == true || Board2.GetComponent<BoardManager>().GetGameState() == true || Board3.GetComponent<BoardManager>().GetGameState() == true){
            if (Board1.GetComponent<BoardManager>().GetGameState() == true){
                Bot1.GetComponent<ChessBot8>().MakeMove();
                yield return new WaitForSeconds(0.1f);
            }
            if (Board1.GetComponent<BoardManager>().GetGameState() == true){
                Bot2.GetComponent<ChessBot8>().MakeMove();
                yield return new WaitForSeconds(0.1f);
            }
            if (Board2.GetComponent<BoardManager>().GetGameState() == true){
                Bot3.GetComponent<ChessBot8>().MakeMove();
                yield return new WaitForSeconds(0.1f);
            }
            if (Board2.GetComponent<BoardManager>().GetGameState() == true){
                Bot4.GetComponent<ChessBot8>().MakeMove();
                yield return new WaitForSeconds(0.1f);
            }
            if (Board3.GetComponent<BoardManager>().GetGameState() == true){
                Bot5.GetComponent<ChessBot8>().MakeMove();
                yield return new WaitForSeconds(0.1f);
            }
            if (Board3.GetComponent<BoardManager>().GetGameState() == true){
                Bot6.GetComponent<ChessBot8>().MakeMove();
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield return new WaitForSeconds(0.1f);
    }

    public GameObject[] RandomizeBots(){
        int Board1Players = 0;
        int Board2Players = 0;
        int Board3Players = 0;
        /*GameObject[] TempBotList = new GameObject[6];
        TempBotList[0] = Bot1;
        TempBotList[1] = Bot2;
        TempBotList[2] = Bot3;
        TempBotList[3] = Bot4;
        TempBotList[4] = Bot5;
        TempBotList[5] = Bot6;*/
        bool HasBoard = false;
        for (int i = 0; i <= BotList.Length; i++){
            while (HasBoard == false){
                int ChosenBoard = Random.Range(1, 4);
                if (ChosenBoard == 1 && Board1Players < 2){
                    BotList[i].GetComponent<ChessBot8>().SetBoard(Board1);
                    HasBoard = true;
                    if (Board1Players == 0){
                        BotList[i].GetComponent<ChessBot8>().SetSide(1);
                    }else{
                        BotList[i].GetComponent<ChessBot8>().SetSide(2);
                    }
                    Board1Players++;
                }else if (ChosenBoard == 2 && Board2Players < 2){
                    BotList[i].GetComponent<ChessBot8>().SetBoard(Board2);
                    HasBoard = true;
                    if (Board2Players == 0){
                        BotList[i].GetComponent<ChessBot8>().SetSide(1);
                    }else{
                        BotList[i].GetComponent<ChessBot8>().SetSide(2);
                    }
                    Board2Players++;
                }else if (ChosenBoard == 3 && Board3Players < 2){
                    BotList[i].GetComponent<ChessBot8>().SetBoard(Board3);
                    HasBoard = true;
                    if (Board3Players == 0){
                        BotList[i].GetComponent<ChessBot8>().SetSide(1);
                    }else{
                        BotList[i].GetComponent<ChessBot8>().SetSide(2);
                    }
                    Board3Players++;
                }

            }
            HasBoard = true;
        }
        return BotList;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        //RandomizeBots();
        Bot1.GetComponent<ChessBot8>().SetBoard(Board1);
        Bot2.GetComponent<ChessBot8>().SetBoard(Board1);
        Bot1.GetComponent<ChessBot8>().SetSide(1);
        Bot2.GetComponent<ChessBot8>().SetSide(2);
        Bot3.GetComponent<ChessBot8>().SetBoard(Board2);
        Bot4.GetComponent<ChessBot8>().SetBoard(Board2);
        Bot3.GetComponent<ChessBot8>().SetSide(1);
        Bot4.GetComponent<ChessBot8>().SetSide(2);
        Bot5.GetComponent<ChessBot8>().SetBoard(Board3);
        Bot6.GetComponent<ChessBot8>().SetBoard(Board3);
        Bot5.GetComponent<ChessBot8>().SetSide(1);
        Bot6.GetComponent<ChessBot8>().SetSide(2);
        //Board1.GetComponent<BoardManager>().SetBoardType1();
        //Board2.GetComponent<BoardManager>().SetBoardType1();
        //Board3.GetComponent<BoardManager>().SetBoardType1();
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown(){
        RunTraining();
    }
}
