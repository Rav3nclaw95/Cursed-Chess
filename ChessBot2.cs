using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBot2 : MonoBehaviour
{
    [SerializeField] int CenterImportanceMult;
    [SerializeField] GameObject BoardManagerObj;
    [SerializeField] int BestMoveX;
    [SerializeField] int BestMoveY;
    [SerializeField] float BestEvalScore;
    [SerializeField] int ReferenceX;
    [SerializeField] int ReferenceY;
    [SerializeField] int PotentialX;
    [SerializeField] int PotentialY;
    [SerializeField] int KingPosX;
    [SerializeField] int KingPosY;
    [SerializeField] int QueenPosX;
    [SerializeField] int QueenPosY;
    [SerializeField] int[] TypeList = new int[16];
    [SerializeField] bool IsOn;
    [SerializeField] int Side;
    [SerializeField] int[] Value = new int[11];
    //[SerializeField] int KingPosX = 0;
    //[SerializeField] int KingPosY = 0;
    private GameObject[,] CurrentObjBoard = new GameObject[8, 8];
    private int[,,] CurrentIntBoard = new int[8, 8, 16];
    private int[,] CurrentHeatBoard = new int[8, 8];
    private int[,] CurrentSideBoard = new int[8, 8];
    private int[,,] CheckingIntBoard = new int[8, 8, 16];
    private int[,,] CheckingIntBoard2 = new int[8, 8, 16];
    private int[,] CheckingHeatBoard = new int[8, 8];
    private int[,] CheckingSideBoard = new int[8, 8];
    private int[,] CheckingSideBoard2 = new int[8, 8];
    private bool[,] CanMoveHereData = new bool[8, 8];
    [SerializeField] int[] DepthCords = new int[12];

    //Clears the CurrentIntBoard
    public void ClearCurrentIntBoard(){
        for (int i = 7; i >= 0; i--){
            for (int t = 7; t >= 0; t--)
            {
                for (int n = 7; n >= 0; n--)
                {
                    CurrentIntBoard[i, t, n] = 0;
                }
            }
        }
    }

    public void ClearCheckingIntBoard(){
        for (int i = 7; i >= 0; i--){
            for (int t = 7; t >= 0; t--)
            {
                for (int n = 7; n >= 0; n--)
                {
                    CheckingIntBoard[i, t, n] = 0;
                }
            }
        }
    }

    public void SetBoard(GameObject Board){
        BoardManagerObj = Board;
    }

    //1= Bot controls White pieces 2= Bot controls Black pieces
    public void SetSide(int side){
        Side = side;
    }

    //Clears the CurrentHeatBoard
    public void ClearCurrentHeatBoard(int[,] UsedHeatBoard){
        for (int i = 7; i >= 0; i--){
            for (int t = 7; t >= 0; t--)
            {
                UsedHeatBoard[i, t] = 0;
            }
        }
    }

    public void ClearCheckingHeatBoard(){
        for (int i = 7; i >= 0; i--){
            for (int t = 7; t >= 0; t--)
            {
                CheckingHeatBoard[i, t] = 0;
            }
        }
    }

    //Sets CurrentIntBoard with current board data and sets CurrentSideBoard with each side is
    public void CreateBoardSnapshot(){
        CurrentObjBoard = BoardManagerObj.GetComponent<BoardManager>().GetBoard();
        ClearCurrentIntBoard();
        bool Side = false;
        //Fills CurrentIntBoard with number data of each piece
        for (int i = 7; i >= 0; i--){
            for (int t = 7; t >= 0; t--)
            {
                if (CurrentObjBoard[i, t] != null){
                    Side = CurrentObjBoard[i, t].GetComponent<PawnMove>().GetColor();
                }else{
                    CurrentSideBoard[i, t] = 0;
                }
                if (Side == true){
                    CurrentSideBoard[i, t] = 1;
                }else if (Side == false){
                    CurrentSideBoard[i, t] = 2;
                }else{
                    CurrentSideBoard[i, t] = 3;
                    Debug.Log("3 Created at position ("+ i +", "+ t +")");
                }
                for (int l = 0; l <= 7; l++){
                    //Debug.Log(CurrentObjBoard[l, 0]);
                    //Debug.Log(CurrentObjBoard[l, 0] + " " + CurrentObjBoard[l, 1] + " " + CurrentObjBoard[l, 2] + " " + CurrentObjBoard[l, 3] + " " + CurrentObjBoard[l, 4] + " " + CurrentObjBoard[l, 5] + " " + CurrentObjBoard[l, 6]);
                }
                if (CurrentObjBoard[i, t] != null){
                    TypeList = CurrentObjBoard[i, t].GetComponent<PawnMove>().GetTypeList();
                    for (int n = 0; n <= TypeList.Length - 1; n++){
                        CurrentIntBoard[i, t, n] = TypeList[n];
                    }
                }
            }
        }
    }

    //Creates a CurrentHeatMap using the CurrentIntBoard 3D List
    public void CreateHeatMap(int[,,] AnalyzedIntBoard, int[,] AnalyzedSideBoard, int[,] OutputHeatBoard){
        ClearCurrentHeatBoard(OutputHeatBoard);
        int Add = -1;
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++)
            {
                if (AnalyzedSideBoard[y, x] == 1){
                    Add = 1;
                }else if (AnalyzedSideBoard[y, x] == 2){
                    Add = -1;
                }
                for (int n = 0; n <= TypeList.Length - 1; n++){
                    //Smiler Code Here: Updated Used Arrays
                    if (AnalyzedIntBoard[y, x, n] == 11){
                        if (y + 1 <= 7 && x + 2 <= 7){
                            OutputHeatBoard[y + 1, x + 2] += 1 * Add;
                        }
                        if (y - 1 >= 0 && x + 2 <= 7){
                            OutputHeatBoard[y - 1, x + 2] += 1 * Add;
                        }
                        if (y + 1 <= 7 && x - 2 >= 0){
                            OutputHeatBoard[y + 1, x - 1] += 1 * Add;
                        }
                        if (y - 1 >= 0 && x - 2 >= 0){
                            OutputHeatBoard[y - 1, x - 2] += 1 * Add;
                        }
                        if (y + 2 <= 7 && x + 1 <= 7){
                            OutputHeatBoard[y + 2, x + 1] += 1 * Add;
                        }
                        if (y + 2 <= 7 && x - 1 >= 0){
                            OutputHeatBoard[y + 2, x - 1] += 1 * Add;
                        }
                        if (y - 2 >= 0 && x + 1 <= 7){
                            OutputHeatBoard[y - 2, x + 1] += 1 * Add;
                        }
                        if (y - 2 >= 0 && x - 1 >= 0){
                            OutputHeatBoard[y - 2, x - 1] += 1 * Add;
                        }
                        if (y + 4 <= 7 && x + 2 <= 7){
                            OutputHeatBoard[y + 4, x + 2] += 1 * Add;
                        }
                        if (y + 4 <= 7 && x - 2 >= 0){
                            OutputHeatBoard[y + 4, x - 2] += 1 * Add;
                        }
                        if (y - 4 >= 0 && x + 2 <= 7){
                            OutputHeatBoard[y - 4, x + 2] += 1 * Add;
                        }
                        if (y - 4 >= 0 && x - 2 >= 0){
                            OutputHeatBoard[y - 4, x - 2] += 1 * Add;
                        }
                        if (y + 2 <= 7 && x + 4 <= 7){
                            OutputHeatBoard[y + 2, x + 4] += 1 * Add;
                        }
                        if (y - 2 >= 0 && x + 4 <= 7){
                            OutputHeatBoard[y - 2, x + 4] += 1 * Add;
                        }
                        if (y + 2 <= 7 && x - 4 >= 0){
                            OutputHeatBoard[y + 2, x + 4] += 1 * Add;
                        }
                        if (y - 2 >= 0 && x - 4 >= 0){
                            OutputHeatBoard[y - 2, x - 4] += 1 * Add;
                        }
                    }    
                    //Portal Thief1 Code Here: Updated Used Arrays
                    else if (AnalyzedIntBoard[y, x, n] == 8){
                        if (y + 1 <= 7 && AnalyzedSideBoard[y, x] != AnalyzedSideBoard[y + 1, x]){
                            OutputHeatBoard[y + 1, x] += 1 * Add;
                        }
                        if (y - 1 >= 0 && AnalyzedSideBoard[y, x] != AnalyzedSideBoard[y - 1, x]){
                            OutputHeatBoard[y - 1, x] += 1 * Add;
                        }
                    }
                    //Portal Thief2 Code Here: Updated Used Arrays
                    else if (AnalyzedIntBoard[y, x, n] == 9){
                        if (y + 1 <= 7){
                            OutputHeatBoard[y + 1, x] += 1 * Add;
                        }
                        if (y + 1 <= 7 && x + 1 <= 7){
                            OutputHeatBoard[y + 1, x + 1] += 1 * Add;
                        }
                        if (y + 1 <= 7 && x - 1 >= 0){
                            OutputHeatBoard[y + 1, x - 1] += 1 * Add;
                        }
                        if (y - 1 >= 0){
                            OutputHeatBoard[y - 1, x] += 1 * Add;
                        }
                        if (y - 1 >= 0 && x + 1 <= 7){
                            OutputHeatBoard[y - 1, x + 1] += 1 * Add;
                        }
                        if (y - 1 >= 0 && x - 1 >= 0){
                            OutputHeatBoard[y - 1, x - 1] += 1 * Add;
                        }
                        if (x + 1 <= 7){
                            OutputHeatBoard[y, x + 1] += 1 * Add;
                        }
                        if (x - 1 >= 0){
                            OutputHeatBoard[y, x - 1] += 1 * Add;
                        }
                    }
                    //Portal Thief3 Code Here: Updated Used Arrays
                    else if (AnalyzedIntBoard[y, x, n] == 10){
                        if (y + 1 <= 7){
                            OutputHeatBoard[y + 1, x] += 1 * Add;
                        }
                        if (y + 1 <= 7 && x + 1 <= 7){
                            OutputHeatBoard[y + 1, x + 1] += 1 * Add;
                        }
                        if (y + 1 <= 7 && x - 1 >= 0){
                            OutputHeatBoard[y + 1, x - 1] += 1 * Add;
                        }
                        if (y - 1 >= 0){
                            OutputHeatBoard[y - 1, x] += 1 * Add;
                        }
                        if (y - 1 >= 0 && x + 1 <= 7){
                            OutputHeatBoard[y - 1, x + 1] += 1 * Add;
                        }
                        if (y - 1 >= 0 && x - 1 >= 0){
                            OutputHeatBoard[y - 1, x - 1] += 1 * Add;
                        }
                        if (x + 1 <= 7){
                            OutputHeatBoard[y, x + 1] += 1 * Add;
                        }
                        if (x - 1 >= 0){
                            OutputHeatBoard[y, x - 1] += 1 * Add;
                        }
                        for (int i = -2; i <= 2; i++){
                            int num = 0;
                            num = x + i;
                            //Debug.Log("X: "+ num +" Y: "+ y + 2);
                            if (y + 2 <= 7 && num >= 0 && num <= 7 && AnalyzedSideBoard[y, x] != AnalyzedSideBoard[y + 2, num]){
                                //Debug.Log("We're in");
                                OutputHeatBoard[y + 2, num] += 1 * Add;
                            }
                        }
                        for (int i = -2; n <= 2; n++){
                            int num2 = 0;
                            num2 = x + i;
                            //Debug.Log("X: "+ num2 +" Y: "+ y + 2);
                            if (y - 2 >= 0 && num2 >= 0 && num2 <= 7 && AnalyzedSideBoard[y, x] != AnalyzedSideBoard[y - 2, num2]){
                                //Debug.Log("We're in");
                                OutputHeatBoard[y - 2, num2] += 1 * Add;
                            }
                        }
                        for (int i = -1; i <= 1; i++){
                            int num = 0;
                            num = y + i;
                            if (num <= 7 && num >= 0 && x - 2 >= 0 && AnalyzedSideBoard[num, x - 2] != AnalyzedSideBoard[num, x - 2]){
                                OutputHeatBoard[num, x - 2] += 1 * Add;
                            }
                        }
                        for (int i = -1; i <= 1; i++){
                            int num = 0;
                            num = y + i;
                            if (num <= 7 && num >= 0 && x + 2 >= 0 && AnalyzedSideBoard[num, x + 2] != AnalyzedSideBoard[num, x + 2]){
                                //Debug.Log("We're in");
                                OutputHeatBoard[num, x + 2] += 1 * Add;
                            }
                        }
                    }
                    //Amazon Code Here: Updated Used Arrays
                    else if (AnalyzedIntBoard[y, x, n] == 7){
                        bool UpRamDone = false;
                        bool DownRamDone = false;
                        bool LeftRamDone = false;
                        bool RightRamDone = false;
                        if (y + 1 <= 7 && x + 2 <= 7){
                            OutputHeatBoard[y + 1, x + 2] += 1 * Add;
                            if (RightRamDone == false){
                                OutputHeatBoard[y, x + 2] += 1 * Add;
                                OutputHeatBoard[y, x + 1] += 1 * Add;
                                RightRamDone = true;
                            }
                        }
                        if (y - 1 >= 0 && x + 2 <= 7){
                            OutputHeatBoard[y - 1, x + 2] += 1 * Add;
                            if (RightRamDone == false){
                                OutputHeatBoard[y, x + 2] += 1 * Add;
                                OutputHeatBoard[y, x + 1] += 1 * Add;
                                RightRamDone = true;
                            }
                        }
                        if (y + 1 <= 7 && x - 2 >= 0){
                            OutputHeatBoard[y + 1, x - 2] += 1 * Add;
                            if (LeftRamDone == false){
                                OutputHeatBoard[y, x - 2] += 1 * Add;
                                OutputHeatBoard[y, x - 1] += 1 * Add;
                                RightRamDone = true;
                            }
                        }
                        if (y - 1 >= 0 && x - 2 >= 0){
                            OutputHeatBoard[y - 1, x - 2] += 1 * Add;
                            if (LeftRamDone == false){
                                OutputHeatBoard[y, x - 2] += 1 * Add;
                                OutputHeatBoard[y, x - 1] += 1 * Add;
                                RightRamDone = true;
                            }
                        }
                        if (y + 2 <= 7 && x + 1 <= 7){
                            OutputHeatBoard[y + 2, x + 1] += 1 * Add;
                            if (UpRamDone == false){
                                OutputHeatBoard[y + 2, x] += 1 * Add;
                                OutputHeatBoard[y + 1, x] += 1 * Add;
                                UpRamDone = true;
                                //Debug.Log("UpRamLocked");
                            }
                        }
                        if (y + 2 <= 7 && x - 1 >= 0){
                            OutputHeatBoard[y + 2, x - 1] += 1 * Add;
                            if (UpRamDone == false){
                                OutputHeatBoard[y + 2, x] += 1 * Add;
                                OutputHeatBoard[y + 1, x] += 1 * Add;
                                UpRamDone = true;
                                //Debug.Log("UpRamLocked");
                            }
                        }
                        if (y - 2 >= 0 && x + 1 <= 7){
                            OutputHeatBoard[y - 2, x + 1] += 1 * Add;
                            OutputHeatBoard[y - 2, x] += 1 * Add;
                            OutputHeatBoard[y - 1, x] += 1 * Add;
                            if (DownRamDone == false){
                                OutputHeatBoard[y - 2, x] += 1 * Add;
                                OutputHeatBoard[y - 1, x] += 1 * Add;
                                UpRamDone = true;
                            }
                        }
                        if (y - 2 >= 0 && x - 1 >= 0){
                            OutputHeatBoard[y - 2, x - 1] += 1 * Add;
                            OutputHeatBoard[y - 2, x] += 1 * Add;
                            OutputHeatBoard[y - 1, x] += 1 * Add;
                            if (DownRamDone == false){
                                OutputHeatBoard[y - 2, x] += 1 * Add;
                                OutputHeatBoard[y - 1, x] += 1 * Add;
                                UpRamDone = true;
                            }
                        }
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (y + i >= 8){
                                break;
                            }
                            if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == 0){
                                OutputHeatBoard[y + i, x] += 1 * Add;
                            }else {
                                if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != 0){
                                    OutputHeatBoard[y + i, x] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess1");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (y + (i * -1) >= 7 || y + (i * -1) <= -1){
                                break;
                            }
                            if (y + (i * -1) <= 7 && y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] == 0){
                                OutputHeatBoard[y + (i * -1), x] += 1 * Add;
                            }else {
                                if (y + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x] != 0){
                                    OutputHeatBoard[y + (i * -1), x] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess2");
                        for (int i = 1; i <= 7; i ++){
                            if (i > 7){
                                break;
                            }
                            if (x + i > 7){
                                break;
                            }

                            if (x + i <= 7 && AnalyzedSideBoard[y, x + i] == 0){
                                OutputHeatBoard[y, x + i] += 1 * Add;
                            }else {
                                if (x + i <= 7 && AnalyzedSideBoard[y, x + i] != 0){
                                    OutputHeatBoard[y, x + i] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess3");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (x + (i * -1) >= 8 || x + (i * -1) <= -1){
                                break;
                            }
                            if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == 0){
                                OutputHeatBoard[y, x + (i * -1)] += 1 * Add;
                            }else {
                                if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] != 0){
                                    OutputHeatBoard[y, x + (i * -1)] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (y + i >= 8 || x + i >= 8){
                                break;
                            }
                            if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] == 0){
                                OutputHeatBoard[y + i, x + i] += 1 * Add;
                            }else {
                                if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != 0){
                                    OutputHeatBoard[y + i, x + i] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess1");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (x + (i * -1) >= 7 || x + (i * -1) <= -1 || y + i >= 8){
                                break;
                            }
                            if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + i <= 7 && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                                OutputHeatBoard[y + i, x + (i * -1)] += 1 * Add;
                            }else {
                                if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + i <= 7 && AnalyzedSideBoard[y + i, x + (i * -1)] != 0){
                                    OutputHeatBoard[y + i, x + (i * -1)] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess2");
                        for (int i = 1; i <= 7; i ++){
                            if (i > 7){
                                break;
                            }
                            if (x + i > 7 || y + (i * -1) <= -1){
                                break;
                            }
                            if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                                OutputHeatBoard[y + (i * -1), x + i] += 1 * Add;
                            }else {
                                if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != 0){
                                    OutputHeatBoard[y + (i * -1), x + i] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess3");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (x + (i * -1) >= 8 || x + (i * -1) <= -1 || y + (i * -1) >= 8 || y + (i * -1) <= -1){
                                break;
                            }
                            if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + (i * -1) <= 7 && y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                                OutputHeatBoard[y + (i * -1), x + (i * -1)] += 1 * Add;
                            }else {
                                if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + (i * -1) <= 7 && y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != 0){
                                    OutputHeatBoard[y + (i * -1), x + (i * -1)] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                    }
                    //Knight Code Here: Updated Used Arrays
                    else if (AnalyzedIntBoard[y, x, n] == 2){
                        bool UpRamDone = false;
                        bool DownRamDone = false;
                        bool LeftRamDone = false;
                        bool RightRamDone = false;
                        if (y + 1 <= 7 && x + 2 <= 7){
                            OutputHeatBoard[y + 1, x + 2] += 1 * Add;
                            if (RightRamDone == false){
                                OutputHeatBoard[y, x + 2] += 1 * Add;
                                OutputHeatBoard[y, x + 1] += 1 * Add;
                                RightRamDone = true;
                            }
                        }
                        if (y - 1 >= 0 && x + 2 <= 7){
                            OutputHeatBoard[y - 1, x + 2] += 1 * Add;
                            if (RightRamDone == false){
                                OutputHeatBoard[y, x + 2] += 1 * Add;
                                OutputHeatBoard[y, x + 1] += 1 * Add;
                                RightRamDone = true;
                            }
                        }
                        if (y + 1 <= 7 && x - 2 >= 0){
                            OutputHeatBoard[y + 1, x - 2] += 1 * Add;
                            if (LeftRamDone == false){
                                OutputHeatBoard[y, x - 2] += 1 * Add;
                                OutputHeatBoard[y, x - 1] += 1 * Add;
                                RightRamDone = true;
                            }
                        }
                        if (y - 1 >= 0 && x - 2 >= 0){
                            OutputHeatBoard[y - 1, x - 2] += 1 * Add;
                            if (LeftRamDone == false){
                                OutputHeatBoard[y, x - 2] += 1 * Add;
                                OutputHeatBoard[y, x - 1] += 1 * Add;
                                RightRamDone = true;
                            }
                        }
                        if (y + 2 <= 7 && x + 1 <= 7){
                            OutputHeatBoard[y + 2, x + 1] += 1 * Add;
                            if (UpRamDone == false){
                                OutputHeatBoard[y + 2, x] += 1 * Add;
                                OutputHeatBoard[y + 1, x] += 1 * Add;
                                UpRamDone = true;
                                //Debug.Log("UpRamLocked");
                            }
                        }
                        if (y + 2 <= 7 && x - 1 >= 0){
                            OutputHeatBoard[y + 2, x - 1] += 1 * Add;
                            if (UpRamDone == false){
                                OutputHeatBoard[y + 2, x] += 1 * Add;
                                OutputHeatBoard[y + 1, x] += 1 * Add;
                                UpRamDone = true;
                                //Debug.Log("UpRamLocked");
                            }
                        }
                        if (y - 2 >= 0 && x + 1 <= 7){
                            OutputHeatBoard[y - 2, x + 1] += 1 * Add;
                            if (DownRamDone == false){
                                OutputHeatBoard[y - 2, x] += 1 * Add;
                                OutputHeatBoard[y - 1, x] += 1 * Add;
                                DownRamDone = true;
                            }
                        }
                        if (y - 2 >= 0 && x - 1 >= 0){
                            OutputHeatBoard[y - 2, x - 1] += 1 * Add;
                            if (DownRamDone == false){
                                OutputHeatBoard[y - 2, x] += 1 * Add;
                                OutputHeatBoard[y - 1, x] += 1 * Add;
                                DownRamDone = true;
                            }
                        }
                    }
                    //Pawn Code Here: Updated Used Arrays
                    else if (AnalyzedIntBoard[y, x, n] == 1){
                        if (AnalyzedSideBoard[y, x] == 1){
                            if (y + 1 <= 7 && x - 1 >= 0){
                                OutputHeatBoard[y + 1, x - 1] += 1 * Add;
                            }
                            if (y + 1 <= 7 && x + 1 <= 7){
                                OutputHeatBoard[y + 1, x + 1] += 1 * Add;
                            }
                        }else if (AnalyzedSideBoard[y, x] == 2){
                            if (y - 1 >= 0 && x - 1 >= 0){
                                OutputHeatBoard[y - 1, x - 1] += 1 * Add;
                            }
                            if (y - 1 >= 0 && x + 1 <= 7){
                                OutputHeatBoard[y - 1, x + 1] += 1 * Add;
                            }
                        }
                    }
                    //King Code Here: Updated Used Arrays
                    else if (AnalyzedIntBoard[y, x, n] == 6){
                        if (y + 1 <= 7){
                            OutputHeatBoard[y + 1, x] += 1 * Add;
                        }
                        if (y + 1 <= 7 && x + 1 <= 7){
                            OutputHeatBoard[y + 1, x + 1] += 1 * Add;
                        }
                        if (y + 1 <= 7 && x - 1 >= 0){
                            OutputHeatBoard[y + 1, x - 1] += 1 * Add;
                        }
                        if (y - 1 >= 0){
                            OutputHeatBoard[y - 1, x] += 1 * Add;
                        }
                        if (y - 1 >= 0 && x + 1 <= 7){
                            OutputHeatBoard[y - 1, x + 1] += 1 * Add;
                        }
                        if (y - 1 >= 0 && x - 1 >= 0){
                            OutputHeatBoard[y - 1, x - 1] += 1 * Add;
                        }
                        if (x + 1 <= 7){
                            OutputHeatBoard[y, x + 1] += 1 * Add;
                        }
                        if (x - 1 >= 0){
                            OutputHeatBoard[y, x - 1] += 1 * Add;
                        }
                    }
                    //Rook Code Here: Updated Used Arrays
                    else if (AnalyzedIntBoard[y, x, n] == 4){
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (y + i >= 8){
                                break;
                            }
                            if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == 0){
                                OutputHeatBoard[y + i, x] += 1 * Add;
                            }else {
                                if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != 0){
                                    OutputHeatBoard[y + i, x] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess1");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (y + (i * -1) >= 7 || y + (i * -1) <= -1){
                                break;
                            }
                            if (y + (i * -1) <= 7 && y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] == 0){
                                OutputHeatBoard[y + (i * -1), x] += 1 * Add;
                            }else {
                                if (y + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x] != 0){
                                    OutputHeatBoard[y + (i * -1), x] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess2");
                        for (int i = 1; i <= 7; i ++){
                            if (i > 7){
                                break;
                            }
                            if (x + i > 7){
                                break;
                            }

                            if (x + i <= 7 && AnalyzedSideBoard[y, x + i] == 0){
                                OutputHeatBoard[y, x + i] += 1 * Add;
                            }else {
                                if (x + i <= 7 && AnalyzedSideBoard[y, x + i] != 0){
                                    OutputHeatBoard[y, x + i] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess3");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (x + (i * -1) >= 8 || x + (i * -1) <= -1){
                                break;
                            }
                            if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == 0){
                                OutputHeatBoard[y, x + (i * -1)] += 1 * Add;
                            }else {
                                if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] != 0){
                                    OutputHeatBoard[y, x + (i * -1)] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                    }
                    //Bishop Code Here: Updated User Arrays
                    else if (AnalyzedIntBoard[y, x, n] == 3){
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (y + i >= 8 || x + i >= 8){
                                break;
                            }
                            if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] == 0){
                                OutputHeatBoard[y + i, x + i] += 1 * Add;
                            }else {
                                if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != 0){
                                    OutputHeatBoard[y + i, x + i] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess1");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (x + (i * -1) >= 7 || x + (i * -1) <= -1 || y + i >= 8){
                                break;
                            }
                            if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + i <= 7 && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                                OutputHeatBoard[y + i, x + (i * -1)] += 1 * Add;
                            }else {
                                if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + i <= 7 && AnalyzedSideBoard[y + i, x + (i * -1)] != 0){
                                    OutputHeatBoard[y + i, x + (i * -1)] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess2");
                        for (int i = 1; i <= 7; i ++){
                            if (i > 7){
                                break;
                            }
                            if (x + i > 7 || y + (i * -1) <= -1){
                                break;
                            }
                            if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                                OutputHeatBoard[y + (i * -1), x + i] += 1 * Add;
                            }else {
                                if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != 0){
                                    OutputHeatBoard[y + (i * -1), x + i] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess3");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (x + (i * -1) >= 8 || x + (i * -1) <= -1 || y + (i * -1) >= 8 || y + (i * -1) <= -1){
                                break;
                            }
                            if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + (i * -1) <= 7 && y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                                OutputHeatBoard[y + (i * -1), x + (i * -1)] += 1 * Add;
                            }else {
                                if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + (i * -1) <= 7 && y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != 0){
                                    OutputHeatBoard[y + (i * -1), x + (i * -1)] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                    }
                    //Queen Code Here: Updated Used Arrays
                    else if (AnalyzedIntBoard[y, x, n] == 5){
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (y + i >= 8 || x + i >= 8){
                                break;
                            }
                            if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] == 0){
                                OutputHeatBoard[y + i, x + i] += 1 * Add;
                            }else {
                                if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != 0){
                                    OutputHeatBoard[y + i, x + i] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess1");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (x + (i * -1) >= 7 || x + (i * -1) <= -1 || y + i >= 8){
                                break;
                            }
                            if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + i <= 7 && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                                OutputHeatBoard[y + i, x + (i * -1)] += 1 * Add;
                            }else {
                                if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + i <= 7 && AnalyzedSideBoard[y + i, x + (i * -1)] != 0){
                                    OutputHeatBoard[y + i, x + (i * -1)] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess2");
                        for (int i = 1; i <= 7; i ++){
                            if (i > 7){
                                break;
                            }
                            if (x + i > 7 || y + (i * -1) <= -1){
                                break;
                            }
                            if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                                OutputHeatBoard[y + (i * -1), x + i] += 1 * Add;
                            }else {
                                if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != 0){
                                    OutputHeatBoard[y + (i * -1), x + i] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess3");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (x + (i * -1) >= 8 || x + (i * -1) <= -1 || y + (i * -1) >= 8 || y + (i * -1) <= -1){
                                break;
                            }
                            if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + (i * -1) <= 7 && y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                                OutputHeatBoard[y + (i * -1), x + (i * -1)] += 1 * Add;
                            }else {
                                if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && y + (i * -1) <= 7 && y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != 0){
                                    OutputHeatBoard[y + (i * -1), x + (i * -1)] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (y + i >= 8){
                                break;
                            }
                            if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == 0){
                                OutputHeatBoard[y + i, x] += 1 * Add;
                            }else {
                                if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != 0){
                                    OutputHeatBoard[y + i, x] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess1");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (y + (i * -1) >= 7 || y + (i * -1) <= -1){
                                break;
                            }
                            if (y + (i * -1) <= 7 && y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] == 0){
                                OutputHeatBoard[y + (i * -1), x] += 1 * Add;
                            }else {
                                if (y + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x] != 0){
                                    OutputHeatBoard[y + (i * -1), x] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess2");
                        for (int i = 1; i <= 7; i ++){
                            if (i > 7){
                                break;
                            }
                            if (x + i > 7){
                                break;
                            }

                            if (x + i <= 7 && AnalyzedSideBoard[y, x + i] == 0){
                                OutputHeatBoard[y, x + i] += 1 * Add;
                            }else {
                                if (x + i <= 7 && AnalyzedSideBoard[y, x + i] != 0){
                                    OutputHeatBoard[y, x + i] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                        //Debug.Log("Sucess3");
                        for (int i = 1; i <= 7; i++){
                            if (i > 7){
                                break;
                            }
                            if (x + (i * -1) >= 8 || x + (i * -1) <= -1){
                                break;
                            }
                            if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == 0){
                                OutputHeatBoard[y, x + (i * -1)] += 1 * Add;
                            }else {
                                if (x + (i * -1) <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] != 0){
                                    OutputHeatBoard[y, x + (i * -1)] += 1 * Add;
                                    break;
                                }else{
                                    break;
                                }
                            }
                        }
                    }
                    //Debug.Log("Piece: "+ AnalyzedIntBoard[y, x, n] +" Side: "+ AnalyzedSideBoard[y, x] + " Pos: ("+ x +", "+ y +") Pawn Pos: "+ OutputHeatBoard[1, 1]);
                }
            }
        }
        /*for (int i = 0; i <= 7; i++){
            Debug.Log(OutputHeatBoard[i, 0] + " " + OutputHeatBoard[i, 1] + " " + OutputHeatBoard[i, 2] + " " + OutputHeatBoard[i, 3] + " " + OutputHeatBoard[i, 4] + " " + OutputHeatBoard[i, 5] + " " + OutputHeatBoard[i, 6] + " " + OutputHeatBoard[i, 7]);
        }*/
    }

    public void CopyCurrentSideBoard(){
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                for (int m = 0; m <= 15; m++){
                    CheckingSideBoard[x, y] = CurrentSideBoard[x, y];
                }
            }
        }
    }

    public void CopyCurrentIntBoard(){
        ClearCheckingIntBoard();
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                for (int m = 0; m <= 15; m++){
                    CheckingIntBoard[x, y, m] = CurrentIntBoard[x, y, m];
                }
            }
        }
    }

    public void ClearMoveHereData(){
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                CanMoveHereData[y, x] = true;
            }
        }
    }

    //0=Nothing 1=Pawn 2=Knight 3=Bishop 4=Rook 5=Queen 6=King 7=Amazon 8=Portal Thief1 9=Portal Thief2 10=Portal Thief3 11=Smiler
    public float NextLegalMove(int[,,] AnalyzedIntBoard, int[,] AnalyzedSideBoard, int x, int y, int m){
            if  (AnalyzedSideBoard[y, x] == Side && AnalyzedIntBoard[y, x, m] != 0){
            //Pawn Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 1){
                if (Side == 1){
                    if (y == 1){
                        if (AnalyzedSideBoard[y + 2, x] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 2);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 2);
                            CanMoveHereData[y + 2, x] = false;
                        }
                    }
                    if (AnalyzedSideBoard[y + 1, x] == 0){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                        CanMoveHereData[y + 1, x] = false;
                    }
                    if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != 0 && HasType(AnalyzedIntBoard[y + 1, x + 1, 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                        CanMoveHereData[y + 1, x + 1] = false;
                    }
                    if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != 0 && HasType(AnalyzedIntBoard[y + 1, x - 1, 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                        CanMoveHereData[y + 1, x - 1] = false;
                    }
                }
                else if (Side == 2){
                   if (y == 6){
                       if (AnalyzedSideBoard[y - 2, x] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 2);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 2);
                            CanMoveHereData[y - 2, x] = false;
                        }
                    }
                    if (y >= 0 && AnalyzedIntBoard[y - 1, x, 0] == 0){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                        CanMoveHereData[y - 1, x] = false;
                    }
                    if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != 0 && HasType(AnalyzedIntBoard[y - 1, x + 1, 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                        CanMoveHereData[y - 1, x + 1] = false;
                    }
                    if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != 0 && HasType(AnalyzedIntBoard[y - 1, x - 1, 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                        CanMoveHereData[y - 1, x - 1] = false;
                    }
                }
                Debug.Log("Pawn: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Knight Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 2){
                if (y + 1 <= 7 && x + 2 <= 7 && (AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 1, x + 2] == 0 || (AnalyzedSideBoard[y + 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y + 1, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    CanMoveHereData[y + 1, x + 2] = false;
                }
                if (y + 1 <= 7 && x - 2 >= 0 && (AnalyzedSideBoard[y + 1, x - 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 1, x - 2] == 0 || (AnalyzedSideBoard[y + 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y + 1, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    CanMoveHereData[y + 1, x - 2] = false;
                }
                if (y - 1 >= 0 && x - 2 >= 0 && (AnalyzedSideBoard[y - 1, x - 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 1, x - 2] == 0 || (AnalyzedSideBoard[y - 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y - 1, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    CanMoveHereData[y - 1, x - 2] = false;
                }
                if (y - 1 >= 0 && x + 2 <= 7 && (AnalyzedSideBoard[y - 1, x + 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 1, x + 2] == 0 || (AnalyzedSideBoard[y - 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y - 1, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    CanMoveHereData[y - 1, x + 2] = false;
                }
                if (y + 2 <= 7 && x + 1 <= 7 && (AnalyzedSideBoard[y + 2, x + 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 2, x + 1] == 0 || (AnalyzedSideBoard[y + 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y + 2, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    CanMoveHereData[y + 2, x + 1] = false;
                }
                if (y + 2 <= 7 && x - 1 >= 0 && (AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 2, x - 1] == 0 || (AnalyzedSideBoard[y + 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y + 2, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    CanMoveHereData[y + 2, x - 1] = false;
                }
                if (y - 2 >= 0 && x + 1 <= 7 && (AnalyzedSideBoard[y - 2, x + 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 2, x + 1] == 0 || (AnalyzedSideBoard[y - 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y - 2, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    CanMoveHereData[y - 2, x + 1] = false;
                }
                if (y - 2 >= 0 && x - 1 >= 0 && (AnalyzedSideBoard[y - 2, x - 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 2, x - 1] == 0 || (AnalyzedSideBoard[y - 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y - 2, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    CanMoveHereData[y - 2, x - 1] = false;
                }
                Debug.Log("Knight: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Bishop Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 3){
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 7 || x + i >= 7){
                            break;
                        }
                        if (y + i <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + i] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CanMoveHereData[y + i, x + i] = false;
                        }else {
                            if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != AnalyzedSideBoard[y, x]){
                                CopyCurrentIntBoard();
                                CopyCurrentSideBoard();
                                MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                                EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                                CanMoveHereData[y + i, x + i] = false;
                                break;
                            }else if (AnalyzedSideBoard[y + i, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y + i, AnalyzedIntBoard) == false && HasType(6, x + i, y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x + i, 0], x, y, AnalyzedIntBoard) == false){
                                CopyCurrentIntBoard();
                                CopyCurrentSideBoard();
                                MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                                EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                                CanMoveHereData[y + i, x + i] = false;
                                break;
                            }
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 8 || x + (i * -1) <= -1){
                            break;
                        }
                        if (y + i <= 7 && x + (i * -1) >= 0 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CanMoveHereData[y + i, x + (i * -1)] = false;
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CanMoveHereData[y + i, x + (i * -1)] = false;
                            break;
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CanMoveHereData[y + i, x + (i * -1)] = false;
                            break;
                        }else {
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (x + i <= 7 || y + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + i] = false;
                        }else if (y + (i * -1) <= 7 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != AnalyzedSideBoard[y, x]){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + i] = false;
                            break;
                        }else if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y  + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + i, y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x + i, 0], x, y, AnalyzedIntBoard) == false){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + i] = false;
                            break;
                        }else {
                            break;
                        }
                        
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + (i * -1) >= 0 || x + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + (i * -1)] = false;
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + (i * -1)] = false;
                            break;
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + (i * -1)] = false;
                            break;
                        }else{
                            break;
                        }
                    }
                    Debug.Log("Bishop: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Rook Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 4){
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + i >= 8){
                        break;
                    }
                    if (y + i <= 7 && CanMoveHereData[y + i, x] == true && AnalyzedSideBoard[y + i, x] == 0){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CanMoveHereData[y + i, x] = false;
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CanMoveHereData[y + i, x] = false;
                        break;
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + i, AnalyzedIntBoard) == false && HasType(6, x, y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x, 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CanMoveHereData[y + i, x] = false;
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess1");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + (i * -1) >= 7 || y + (i * -1) <= -1){
                        break;
                    }
                    if (y + (i * -1) >= 0 && CanMoveHereData[y + (i * -1), x] == true && AnalyzedSideBoard[y + (i * -1), x] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        CanMoveHereData[y + (i * -1), x] = false;
                    }else {
                        if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + (i * -1), x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x, 0], x, y, AnalyzedIntBoard) == false)){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                            CanMoveHereData[y + (i * -1), x] = false;
                            break;
                        }else{
                            break;
                        }
                    }
                }
                for (int i = 1; i <= 7; i ++){
                    if (i > 7){
                        break;
                    }
                    if (x + i > 7){
                        break;
                    }
                    if (x + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == 0 && HasType(4, x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CanMoveHereData[y, x + i] = false;
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CanMoveHereData[y, x + i] = false;
                        break;
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y, AnalyzedIntBoard) == false && HasType(6, x + i, y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + i, 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CanMoveHereData[y, x + i] = false;
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess3");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (x + (i * -1) <= -1){
                        break;
                    }
                    if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CanMoveHereData[y, x + (i * -1)] = false;
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CanMoveHereData[y, x + (i * -1)] = false;
                        break;
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CanMoveHereData[y, x + (i * -1)] = false;
                        break;
                    }else{
                        break;
                    }
                    
                }
                Debug.Log("Rook: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Queen Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 5){
                //Diagonal Code
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 7 || x + i >= 7){
                            break;
                        }
                        if (y + i <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + i] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CanMoveHereData[y + i, x + i] = false;
                        }else if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != AnalyzedSideBoard[y, x]){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CanMoveHereData[y + i, x + i] = false;
                            break;
                        }else{
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 8 || x + (i * -1) <= -1){
                            break;
                        }
                        if (y + i <= 7 && x + (i * -1) >= 0 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CanMoveHereData[y + i, x + (i * -1)] = false;
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CanMoveHereData[y + i, x + (i * -1)] = false;
                            break;
                        }else {
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (x + i <= 7 || y + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + i] = false;
                        }else if (y + (i * -1) <= 7 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != AnalyzedSideBoard[y, x]){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + i] = false;
                            break;
                        }else {
                            break;
                        }
                        
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + (i * -1) >= 0 || x + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + (i * -1)] = false;
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + (i * -1)] = false;
                            break;
                        }else{
                            break;
                        }
                    }
                //Straight Code
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + i >= 8){
                        break;
                    }
                    if (y + i <= 7 && CanMoveHereData[y + i, x] == true && AnalyzedSideBoard[y + i, x] == 0){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CanMoveHereData[y + i, x] = false;
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CanMoveHereData[y + i, x] = false;
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess1");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + (i * -1) >= 7 || y + (i * -1) <= -1){
                        break;
                    }
                    if (y + (i * -1) >= 0 && CanMoveHereData[y + (i * -1), x] == true && AnalyzedSideBoard[y + (i * -1), x] == 0){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        CanMoveHereData[y + (i * -1), x] = false;
                    }else if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        CanMoveHereData[y + (i * -1), x] = false;
                        break;
                    }else{
                        break;
                    }
                }
                for (int i = 1; i <= 7; i ++){
                    if (i > 7){
                        break;
                    }
                    if (x + i > 7){
                        break;
                    }
                    if (x + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == 0 && HasType(4, x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CanMoveHereData[y, x + i] = false;
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CanMoveHereData[y, x + i] = false;
                        break;
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y, AnalyzedIntBoard) == false && HasType(6, x + i, y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + i, 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CanMoveHereData[y, x + i] = false;
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess3");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (x + (i * -1) <= -1){
                        break;
                    }
                    if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CanMoveHereData[y, x + (i * -1)] = false;
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CanMoveHereData[y, x + (i * -1)] = false;
                        break;
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CanMoveHereData[y, x + (i * -1)] = false;
                        break;
                    }else{
                        break;
                    }
                    
                }
                Debug.Log("Queen: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //King Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 6){
                if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CanMoveHereData[y + 1, x] = false;
                }
                if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    CanMoveHereData[y + 1, x + 1] = false;
                }
                if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    CanMoveHereData[y + 1, x - 1] = false;
                }
                if (y - 1 >= 0 && AnalyzedSideBoard[y - 1, x] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CanMoveHereData[y - 1, x] = false;
                }
                if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    CanMoveHereData[y - 1, x + 1] = false;
                }
                if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    CanMoveHereData[y - 1, x - 1] = false;
                }
                if (x + 1 <= 7 && AnalyzedSideBoard[y, x + 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    CanMoveHereData[y, x + 1] = false;
                }
                if (x - 1 >= 0 && AnalyzedSideBoard[y, x - 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    CanMoveHereData[y, x - 1] = false;
                }
                Debug.Log("King: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Amazon Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 7){
                //Knight part of logic
                if (y + 1 <= 7 && x + 2 <= 7 && (AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 1, x + 2] == 0 || (AnalyzedSideBoard[y + 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y + 1, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    CanMoveHereData[y + 1, x + 2] = false;
                }
                if (y + 1 <= 7 && x - 2 >= 0 && (AnalyzedSideBoard[y + 1, x - 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 1, x - 2] == 0 || (AnalyzedSideBoard[y + 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y + 1, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    CanMoveHereData[y + 1, x - 2] = false;
                }
                if (y - 1 >= 0 && x - 2 >= 0 && (AnalyzedSideBoard[y - 1, x - 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 1, x - 2] == 0 || (AnalyzedSideBoard[y - 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y - 1, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    CanMoveHereData[y - 1, x - 2] = false;
                }
                if (y - 1 >= 0 && x + 2 <= 7 && (AnalyzedSideBoard[y - 1, x + 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 1, x + 2] == 0 || (AnalyzedSideBoard[y - 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y - 1, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    CanMoveHereData[y - 1, x + 2] = false;
                }
                if (y + 2 <= 7 && x + 1 <= 7 && (AnalyzedSideBoard[y + 2, x + 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 2, x + 1] == 0 || (AnalyzedSideBoard[y + 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y + 2, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    CanMoveHereData[y + 2, x + 1] = false;
                }
                if (y + 2 <= 7 && x - 1 >= 0 && (AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 2, x - 1] == 0 || (AnalyzedSideBoard[y + 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y + 2, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    CanMoveHereData[y + 2, x - 1] = false;
                }
                if (y - 2 >= 0 && x + 1 <= 7 && (AnalyzedSideBoard[y - 2, x + 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 2, x + 1] == 0 || (AnalyzedSideBoard[y - 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y - 2, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    CanMoveHereData[y - 2, x + 1] = false;
                }
                if (y - 2 >= 0 && x - 1 >= 0 && (AnalyzedSideBoard[y - 2, x - 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 2, x - 1] == 0 || (AnalyzedSideBoard[y - 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y - 2, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    CanMoveHereData[y - 2, x - 1] = false;
                }
                //Diagonal Code
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 7 || x + i >= 7){
                            break;
                        }
                        if (y + i <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + i] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CanMoveHereData[y + i, x + i] = false;
                        }else {
                            if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != AnalyzedSideBoard[y, x]){
                                CopyCurrentIntBoard();
                                CopyCurrentSideBoard();
                                MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                                EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                                CanMoveHereData[y + i, x + i] = false;
                                break;
                            }else if (AnalyzedSideBoard[y + i, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y + i, AnalyzedIntBoard) == false && HasType(6, x + i, y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x + i, 0], x, y, AnalyzedIntBoard) == false){
                                CopyCurrentIntBoard();
                                CopyCurrentSideBoard();
                                MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                                EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                                CanMoveHereData[y + i, x + i] = false;
                                break;
                            }
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 8 || x + (i * -1) <= -1){
                            break;
                        }
                        if (y + i <= 7 && x + (i * -1) >= 0 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CanMoveHereData[y + i, x + (i * -1)] = false;
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CanMoveHereData[y + i, x + (i * -1)] = false;
                            break;
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CanMoveHereData[y + i, x + (i * -1)] = false;
                            break;
                        }else {
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (x + i <= 7 || y + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + i] = false;
                        }else if (y + (i * -1) <= 7 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != AnalyzedSideBoard[y, x]){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + i] = false;
                            break;
                        }else if (y + (i * -1) <= 7 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y  + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + i, y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x + i, 0], x, y, AnalyzedIntBoard) == false){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + i] = false;
                            break;
                        }else {
                            break;
                        }
                        
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + (i * -1) >= 0 || x + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + (i * -1)] = false;
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + (i * -1)] = false;
                            break;
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CanMoveHereData[y + (i * -1), x + (i * -1)] = false;
                            break;
                        }else{
                            break;
                        }
                    }
                //Straight Code
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + i >= 8){
                        break;
                    }
                    if (y + i <= 7 && CanMoveHereData[y + i, x] == true && AnalyzedSideBoard[y + i, x] == 0){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CanMoveHereData[y + i, x] = false;
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CanMoveHereData[y + i, x] = false;
                        break;
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + i, AnalyzedIntBoard) == false && HasType(6, x, y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x, 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CanMoveHereData[y + i, x] = false;
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess1");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + (i * -1) >= 7 || y + (i * -1) <= -1){
                        break;
                    }
                    if (y + (i * -1) >= 0 && CanMoveHereData[y + (i * -1), x] == true && AnalyzedSideBoard[y + (i * -1), x] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        CanMoveHereData[y + (i * -1), x] = false;
                    }else {
                        if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + (i * -1), x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x, 0], x, y, AnalyzedIntBoard) == false)){
                            CopyCurrentIntBoard();
                            CopyCurrentSideBoard();
                            MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                            EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                            CanMoveHereData[y + (i * -1), x] = false;
                            break;
                        }else{
                            break;
                        }
                    }
                }
                for (int i = 1; i <= 7; i ++){
                    if (i > 7){
                        break;
                    }
                    if (x + i > 7){
                        break;
                    }
                    if (x + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == 0 && HasType(4, x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CanMoveHereData[y, x + i] = false;
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CanMoveHereData[y, x + i] = false;
                        break;
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y, AnalyzedIntBoard) == false && HasType(6, x + i, y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + i, 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CanMoveHereData[y, x + i] = false;
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess3");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (x + (i * -1) <= -1){
                        break;
                    }
                    if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CanMoveHereData[y, x + (i * -1)] = false;
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CanMoveHereData[y, x + (i * -1)] = false;
                        break;
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CanMoveHereData[y, x + (i * -1)] = false;
                        break;
                    }else{
                        break;
                    }
                    
                }
                Debug.Log("Amazon: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Portal Thief 1 Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 8){
                if (y + 1 <= 7 && AnalyzedSideBoard[y, x] != AnalyzedSideBoard[y + 1, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CanMoveHereData[y + 1, x] = false;
                }else if (y + 1 <= 7 && AnalyzedSideBoard[y, x]== AnalyzedSideBoard[y - 1, x] && HasType(1, x, y - 1, AnalyzedIntBoard)){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CanMoveHereData[y - 1, x] = false;
                }
                Debug.Log("Portal Thief 1: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Portal Thief 2 Logic Code
            if (AnalyzedIntBoard[y, x, m] == 9){
                if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CanMoveHereData[y + 1, x] = false;
                }
                if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    CanMoveHereData[y + 1, x + 1] = false;
                }
                if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    CanMoveHereData[y + 1, x - 1] = false;
                }
                if (y - 1 >= 0 && AnalyzedSideBoard[y - 1, x] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CanMoveHereData[y - 1, x] = false;
                }
                if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    CanMoveHereData[y - 1, x + 1] = false;
                }
                if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    CanMoveHereData[y - 1, x - 1] = false;
                }
                if (x + 1 <= 7 && AnalyzedSideBoard[y, x + 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    CanMoveHereData[y, x + 1] = false;
                }
                if (x - 1 >= 0 && AnalyzedSideBoard[y, x - 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    CanMoveHereData[y, x - 1] = false;
                }
                Debug.Log("Portal Thief 2: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Portal Thief 3 Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 10){
                if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CanMoveHereData[y + 1, x] = false;
                }
                if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    CanMoveHereData[y + 1, x + 1] = false;
                }
                if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    CanMoveHereData[y + 1, x - 1] = false;
                }
                if (y - 1 >= 0 && AnalyzedSideBoard[y - 1, x] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CanMoveHereData[y - 1, x] = false;
                }
                if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    CanMoveHereData[y - 1, x + 1] = false;
                }
                if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    CanMoveHereData[y - 1, x - 1] = false;
                }
                if (x + 1 <= 7 && AnalyzedSideBoard[y, x + 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    CanMoveHereData[y, x + 1] = false;
                }
                if (x - 1 >= 0 && AnalyzedSideBoard[y, x - 1] != Side){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    CanMoveHereData[y, x - 1] = false;
                }
                for (int i = -2; i <= 2; i++){
                    int num = 0;
                    num = x + i;
                    //Debug.Log("X: "+ num +" Y: "+ y + 2);
                    if (y + 2 <= 7 && num >= 0 && num <= 7 && AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + 2);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + 2);
                        CanMoveHereData[y + 2, x + i] = false;
                    }
                }
                for (int i = -2; i <= 2; i++){
                    int num2 = 0;
                    num2 = x + i;
                    //Debug.Log("X: "+ num2 +" Y: "+ y + 2);
                    if (y - 2 >= 0 && num2 >= 0 && num2 <= 7 && AnalyzedSideBoard[y - 2, x + i] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y - 2);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y - 2);
                        CanMoveHereData[y - 2, x + i] = false;
                    }
                }
                for (int i = -1; i <= 1; i++){
                    int num = 0;
                    num = y + i;
                    if (num <= 7 && num >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y + i, x - 2] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + i);
                        CanMoveHereData[y + i, x - 2] = false;
                    }
                }
                for (int i = -1; i <= 1; i++){
                    int num = 0;
                    num = y + i;
                    if (num <= 7 && num >= 0 && x + 2 >= 0 && AnalyzedSideBoard[y + i, x + 2] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + i);
                        CanMoveHereData[y + i, x + 2] = false;
                    }
                }
                Debug.Log("Portal Thief 3: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Smiler Logic Code
            if (AnalyzedIntBoard[y, x, m] == 11){
                if (y + 1 <= 7 && x + 2 <= 7 && (AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    CanMoveHereData[y + 1, x + 2] = false;
                }
                if (y + 1 <= 7 && x - 2 <= 7 && (AnalyzedSideBoard[y + 1, x - 2] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    CanMoveHereData[y + 1, x - 2] = false;
                }
                if (y - 1 <= 7 && x - 2 <= 7 && (AnalyzedSideBoard[y - 1, x - 2] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y - 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    CanMoveHereData[y - 1, x - 2] = false;
                }
                if (y - 1 <= 7 && x + 2 <= 7 && (AnalyzedSideBoard[y - 1, x + 2] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y - 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    CanMoveHereData[y - 1, x + 2] = false;
                }
                if (y + 1 <= 7 && x + 2 <= 7 && (AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    CanMoveHereData[y + 1, x + 2] = false;
                }
                if (y + 2 <= 7 && x + 1 <= 7 && (AnalyzedSideBoard[y + 2, x + 1] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    CanMoveHereData[y + 2, x + 1] = false;
                }
                if (y + 2 <= 7 && x - 1 <= 7 && (AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    CanMoveHereData[y + 2, x - 1] = false;
                }
                if (y - 2 <= 7 && x + 1 <= 7 && (AnalyzedSideBoard[y - 2, x + 1] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y - 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    CanMoveHereData[y - 2, x + 1] = false;
                }
                if (y - 2 <= 7 && x - 1 <= 7 && (AnalyzedSideBoard[y - 2, x - 1] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y - 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    CanMoveHereData[y - 2, x - 1] = false;
                }
                if (y + 4 <= 7 && x + 2 <= 7 && AnalyzedSideBoard[y + 4, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 4);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 4);
                    CanMoveHereData[y + 4, x + 2] = false;
                }
                if (y + 4 <= 7 && x - 2 >= 0 && AnalyzedSideBoard[y + 4, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 4);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 4);
                    CanMoveHereData[y + 4, x - 2] = false;
                }
                if (y - 4 >= 0 && x + 2 <= 7 && AnalyzedSideBoard[y - 4, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 4);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 4);
                    CanMoveHereData[y - 4, x + 2] = false;
                }
                if (y - 4 >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y - 4, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 4);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 4);
                    CanMoveHereData[y - 4, x - 2] = false;
                }
                if (y + 2 <= 7 && x + 4 <= 7 && AnalyzedSideBoard[y + 2, x + 4] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y + 2);
                    CanMoveHereData[y + 2, x + 4] = false;
                }
                if (y - 2 >= 0 && x + 4 <= 7 && AnalyzedSideBoard[y - 2, x + 4] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y - 2);
                    CanMoveHereData[y - 2, x + 4] = false;
                }
                if (y + 2 <= 7 && x - 4 >= 0 && AnalyzedSideBoard[y + 2, x - 4] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y + 2);
                    CanMoveHereData[y + 2, x - 4] = false;
                }
                if (y - 2 >= 0 && x - 4 >= 0 && AnalyzedSideBoard[y - 2, x - 4] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y - 2);
                    CanMoveHereData[y - 2, x - 4] = false;
                }
                Debug.Log("Smiler: Position of Checked Piece: ("+ x +", "+ y +")");
            }else{
                //Debug.Log("No Correct Piece found Pos: (" + x +", "+ y +")");
            }
            }
        return 0;
    }

    public bool IsLegalCaptureMove(int[,,] AnalyzedIntBoard, int[,] AnalyzedSideBoard, int x, int y, int FutX, int FutY){
        for (int m = 0; m <= 15; m++){
            if (AnalyzedIntBoard[y, x, m] == 0){
                break;
            }
            //Pawn Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 1){
                if (Side == 1){
                    if (y == 1){
                        if (AnalyzedSideBoard[y + 2, x] == 0){
                            if (y + 2 == FutY && x == FutX){
                                return true;
                            }
                        }
                    }
                    if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] == 0){
                        if (y + 1 == FutY && x == FutX){
                            return true;
                        }
                    }
                    if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != 0 && HasType(AnalyzedIntBoard[y + 1, x + 1, 0], x, y, AnalyzedIntBoard)){
                        if (y + 1 == FutY && x + 1 == FutX){
                            return true;
                        }
                    }
                    if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != 0 && HasType(AnalyzedIntBoard[y + 1, x - 1, 0], x, y, AnalyzedIntBoard)){
                        if (y + 1 == FutY && x - 1 == FutX){
                            return true;
                        }
                    }
                }
                else if (Side == 2){
                   if (y == 6){
                       if (AnalyzedSideBoard[y - 2, x] == 0){
                            if (y - 2 == FutY && x == FutX){
                                return true;
                            }
                        }
                    }
                    if (y - 1 >= 0 && AnalyzedIntBoard[y - 1, x, 0] == 0){
                        if (y - 1 == FutY && x == FutX){
                            return true;
                        }
                    }
                    if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != 0 && HasType(AnalyzedIntBoard[y - 1, x + 1, 0], x, y, AnalyzedIntBoard) == false){
                        if (y - 1 == FutY && x + 1 == FutX){
                            return true;
                        }
                    }
                    if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != 0 && HasType(AnalyzedIntBoard[y - 1, x - 1, 0], x, y, AnalyzedIntBoard) == false){
                        if (y - 1 == FutY && x - 1 == FutX){
                            return true;
                        }
                    }
                }
                //Debug.Log("Pawn: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Knight Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 2){
                if (y + 1 <= 7 && x + 2 <= 7 && (AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 1, x + 2] == 0 || (AnalyzedSideBoard[y + 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y + 1, AnalyzedIntBoard) == false))){
                    if (y + 1 == FutY && x + 2 == FutX){
                        return true;
                    }
                }
                if (y + 1 <= 7 && x - 2 >= 0 && (AnalyzedSideBoard[y + 1, x - 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 1, x - 2] == 0 || (AnalyzedSideBoard[y + 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y + 1, AnalyzedIntBoard) == false))){
                    if (y + 1 == FutY && x - 2 == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && x - 2 >= 0 && (AnalyzedSideBoard[y - 1, x - 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 1, x - 2] == 0 || (AnalyzedSideBoard[y - 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y - 1, AnalyzedIntBoard) == false))){
                    if (y - 1 == FutY && x - 2 == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && x + 2 <= 7 && (AnalyzedSideBoard[y - 1, x + 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 1, x + 2] == 0 || (AnalyzedSideBoard[y - 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y - 1, AnalyzedIntBoard) == false))){
                    if (y - 1 == FutY && x + 2 == FutX){
                        return true;
                    }
                }
                if (y + 2 <= 7 && x + 1 <= 7 && (AnalyzedSideBoard[y + 2, x + 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 2, x + 1] == 0 || (AnalyzedSideBoard[y + 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y + 2, AnalyzedIntBoard) == false))){
                    if (y + 2 == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (y + 2 <= 7 && x - 1 >= 0 && (AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 2, x - 1] == 0 || (AnalyzedSideBoard[y + 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y + 2, AnalyzedIntBoard) == false))){
                    if (y + 2 == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                if (y - 2 >= 0 && x + 1 <= 7 && (AnalyzedSideBoard[y - 2, x + 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 2, x + 1] == 0 || (AnalyzedSideBoard[y - 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y - 2, AnalyzedIntBoard) == false))){
                    if (y - 2 == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (y - 2 >= 0 && x - 1 >= 0 && (AnalyzedSideBoard[y - 2, x - 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 2, x - 1] == 0 || (AnalyzedSideBoard[y - 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y - 2, AnalyzedIntBoard) == false))){
                    if (y - 2 == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                //Debug.Log("Knight: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Bishop Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 3){
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 7 || x + i >= 7){
                            break;
                        }
                        if (y + i <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + i] == 0){
                            if (y + i == FutY && x + i == FutX){
                                return true;
                            }
                        }else {
                            if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != AnalyzedSideBoard[y, x]){
                                if (y + i == FutY && x + i == FutX){
                                    return true;
                                }
                                break;
                            }else if (AnalyzedSideBoard[y + i, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y + i, AnalyzedIntBoard) == false && HasType(6, x + i, y + i, AnalyzedIntBoard) == false && HasType(3, x + i, y + i, AnalyzedIntBoard) == false){
                                if (y + i == FutY && x + i == FutX){
                                    return true;
                                }
                                break;
                            }
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 8 || x + (i * -1) <= -1){
                            break;
                        }
                        if (y + i <= 7 && x + (i * -1) >= 0 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                            if (y + i == FutY && x + (i * -1) == FutX){
                                return true;
                            }
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            if (y + i == FutY && x + (i * -1) == FutX){
                                return true;
                            }
                            break;
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(3, x + (i * -1), y + i, AnalyzedIntBoard) == false){
                            if (y + i == FutY && x + (i * -1) == FutX){
                                return true;
                            }
                            break;
                        }else {
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (x + i <= 7 || y + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                            if (y + (i * -1) == FutY && x + i == FutX){
                                return true;
                            }
                        }else if (y + (i * -1) <= 7 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != AnalyzedSideBoard[y, x]){
                            if (y + (i * -1) == FutY && x + i == FutX){
                                return true;
                            }
                            break;
                        }else if (y + (i * -1) <= 7 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y  + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + i, y + (i * -1), AnalyzedIntBoard) == false && HasType(3, x + i, y + (i * -1), AnalyzedIntBoard) == false){
                            if (y + (i * -1) == FutY && x + i == FutX){
                                return true;
                            }
                            break;
                        }else {
                            break;
                        }
                        
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + (i * -1) >= 0 || x + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                            if (y + (i * -1) == FutY && x + (i * -1) ==FutX){
                                return true;
                            }
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            if (y + (i * -1) == FutY && x + (i * -1) ==FutX){
                                return true;
                            }
                            break;
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(3, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false){
                            if (y + (i * -1) == FutY && x + (i * -1) ==FutX){
                                return true;
                            }
                            break;
                        }else{
                            break;
                        }
                    }
                    //Debug.Log("Bishop: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Rook Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 4){
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + i >= 8){
                        break;
                    }
                    if (y + i <= 7 && CanMoveHereData[y + i, x] == true && AnalyzedSideBoard[y + i, x] == 0){
                        if (y + i == FutY && x == FutX){
                            return true;
                        }
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != AnalyzedSideBoard[y, x]){
                        if (y + i == FutY && x == FutX){
                            return true;
                        }
                        break;
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + i, AnalyzedIntBoard) == false && HasType(6, x, y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x, 0], x, y, AnalyzedIntBoard) == false){
                        if (y + i == FutY && x == FutX){
                            return true;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess1");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + (i * -1) >= 7 || y + (i * -1) <= -1){
                        break;
                    }
                    if (y + (i * -1) >= 0 && CanMoveHereData[y + (i * -1), x] == true && AnalyzedSideBoard[y + (i * -1), x] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        if (y + (i * -1) == FutY && x == FutX){
                            return true;
                        }
                    }else {
                        if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + (i * -1), x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x, 0], x, y, AnalyzedIntBoard) == false)){
                            if (y + (i * -1) == FutY && x == FutX){
                                return true;
                            }
                            break;
                        }else{
                            break;
                        }
                    }
                }
                for (int i = 1; i <= 7; i ++){
                    if (i > 7){
                        break;
                    }
                    if (x + i > 7){
                        break;
                    }
                    if (x + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == 0 && HasType(4, x, y, AnalyzedIntBoard) == false){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                        break;
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y, AnalyzedIntBoard) == false && HasType(6, x + i, y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + i, 0], x, y, AnalyzedIntBoard) == false){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess3");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (x + (i * -1) <= -1){
                        break;
                    }
                    if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                        break;
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                        break;
                    }else{
                        break;
                    }
                    
                }
                //Debug.Log("Rook: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Queen Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 5){
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 7 || x + i >= 7){
                            break;
                        }
                        if (y + i <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + i] == 0){
                            if (y + i == FutY && x + i == FutX){
                                return true;
                            }
                        }else {
                            if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != AnalyzedSideBoard[y, x]){
                                if (y + i == FutY && x + i == FutX){
                                    return true;
                                }
                                break;
                            }else if (AnalyzedSideBoard[y + i, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y + i, AnalyzedIntBoard) == false && HasType(6, x + i, y + i, AnalyzedIntBoard) == false && HasType(3, x + i, y + i, AnalyzedIntBoard) == false){
                                if (y + i == FutY && x + i == FutX){
                                    return true;
                                }
                                break;
                            }
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 8 || x + (i * -1) <= -1){
                            break;
                        }
                        if (y + i <= 7 && x + (i * -1) >= 0 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                            if (y + i == FutY && x + (i * -1) == FutX){
                                return true;
                            }
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            if (y + i == FutY && x + (i * -1) == FutX){
                                return true;
                            }
                            break;
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(3, x + (i * -1), y + i, AnalyzedIntBoard) == false){
                            if (y + i == FutY && x + (i * -1) == FutX){
                                return true;
                            }
                            break;
                        }else {
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (x + i <= 7 || y + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                            if (y + (i * -1) == FutY && x + i == FutX){
                                return true;
                            }
                        }else if (y + (i * -1) <= 7 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != AnalyzedSideBoard[y, x]){
                            if (y + (i * -1) == FutY && x + i == FutX){
                                return true;
                            }
                            break;
                        }else if (y + (i * -1) <= 7 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y  + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + i, y + (i * -1), AnalyzedIntBoard) == false && HasType(3, x + i, y + (i * -1), AnalyzedIntBoard) == false){
                            if (y + (i * -1) == FutY && x + i == FutX){
                                return true;
                            }
                            break;
                        }else {
                            break;
                        }
                        
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + (i * -1) >= 0 || x + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                            if (y + (i * -1) == FutY && x + (i * -1) ==FutX){
                                return true;
                            }
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            if (y + (i * -1) == FutY && x + (i * -1) ==FutX){
                                return true;
                            }
                            break;
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(3, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false){
                            if (y + (i * -1) == FutY && x + (i * -1) ==FutX){
                                return true;
                            }
                            break;
                        }else{
                            break;
                        }
                    }
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + i >= 8){
                        break;
                    }
                    if (y + i <= 7 && CanMoveHereData[y + i, x] == true && AnalyzedSideBoard[y + i, x] == 0){
                        if (y + i == FutY && x == FutX){
                            return true;
                        }
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != AnalyzedSideBoard[y, x]){
                        if (y + i == FutY && x == FutX){
                            return true;
                        }
                        break;
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + i, AnalyzedIntBoard) == false && HasType(6, x, y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x, 0], x, y, AnalyzedIntBoard) == false){
                        if (y + i == FutY && x == FutX){
                            return true;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess1");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + (i * -1) >= 7 || y + (i * -1) <= -1){
                        break;
                    }
                    if (y + (i * -1) >= 0 && CanMoveHereData[y + (i * -1), x] == true && AnalyzedSideBoard[y + (i * -1), x] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        if (y + (i * -1) == FutY && x == FutX){
                            return true;
                        }
                    }else {
                        if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + (i * -1), x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x, 0], x, y, AnalyzedIntBoard) == false)){
                            if (y + (i * -1) == FutY && x == FutX){
                                return true;
                            }
                            break;
                        }else{
                            break;
                        }
                    }
                }
                for (int i = 1; i <= 7; i ++){
                    if (i > 7){
                        break;
                    }
                    if (x + i > 7){
                        break;
                    }
                    if (x + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == 0 && HasType(4, x, y, AnalyzedIntBoard) == false){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                        break;
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y, AnalyzedIntBoard) == false && HasType(6, x + i, y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + i, 0], x, y, AnalyzedIntBoard) == false){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess3");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (x + (i * -1) <= -1){
                        break;
                    }
                    if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                        break;
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                        break;
                    }else{
                        break;
                    }
                    
                }

                //Debug.Log("Queen: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //King Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 6){
                if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] != Side){
                    if (y + 1 == FutY && x == FutX){
                        return true;
                    }
                }
                if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != Side){
                    if (y + 1 == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != Side){
                    if (y + 1 == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && AnalyzedSideBoard[y - 1, x] != Side){
                    if (y - 1 == FutY && x == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != Side){
                    if (y - 1 == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != Side){
                    if (y - 1 == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                if (x + 1 <= 7 && AnalyzedSideBoard[y, x + 1] != Side){
                    if (y == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (x - 1 >= 0 && AnalyzedSideBoard[y, x - 1] != Side){
                    if (y == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                //Debug.Log("King: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Amazon Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 7){
                if (y + 1 <= 7 && x + 2 <= 7 && (AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 1, x + 2] == 0 || (AnalyzedSideBoard[y + 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y + 1, AnalyzedIntBoard) == false))){
                    if (y + 1 == FutY && x + 2 == FutX){
                        return true;
                    }
                }
                if (y + 1 <= 7 && x - 2 >= 0 && (AnalyzedSideBoard[y + 1, x - 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 1, x - 2] == 0 || (AnalyzedSideBoard[y + 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y + 1, AnalyzedIntBoard) == false))){
                    if (y + 1 == FutY && x - 2 == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && x - 2 >= 0 && (AnalyzedSideBoard[y - 1, x - 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 1, x - 2] == 0 || (AnalyzedSideBoard[y - 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y - 1, AnalyzedIntBoard) == false))){
                    if (y - 1 == FutY && x - 2 == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && x + 2 <= 7 && (AnalyzedSideBoard[y - 1, x + 2] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 1, x + 2] == 0 || (AnalyzedSideBoard[y - 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y - 1, AnalyzedIntBoard) == false))){
                    if (y - 1 == FutY && x + 2 == FutX){
                        return true;
                    }
                }
                if (y + 2 <= 7 && x + 1 <= 7 && (AnalyzedSideBoard[y + 2, x + 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 2, x + 1] == 0 || (AnalyzedSideBoard[y + 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y + 2, AnalyzedIntBoard) == false))){
                    if (y + 2 == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (y + 2 <= 7 && x - 1 >= 0 && (AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y + 2, x - 1] == 0 || (AnalyzedSideBoard[y + 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y + 2, AnalyzedIntBoard) == false))){
                    if (y + 2 == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                if (y - 2 >= 0 && x + 1 <= 7 && (AnalyzedSideBoard[y - 2, x + 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 2, x + 1] == 0 || (AnalyzedSideBoard[y - 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y - 2, AnalyzedIntBoard) == false))){
                    if (y - 2 == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (y - 2 >= 0 && x - 1 >= 0 && (AnalyzedSideBoard[y - 2, x - 1] != AnalyzedSideBoard[y, x] || AnalyzedSideBoard[y - 2, x - 1] == 0 || (AnalyzedSideBoard[y - 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y - 2, AnalyzedIntBoard) == false))){
                    if (y - 2 == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 7 || x + i >= 7){
                            break;
                        }
                        if (y + i <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + i] == 0){
                            if (y + i == FutY && x + i == FutX){
                                return true;
                            }
                        }else {
                            if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != AnalyzedSideBoard[y, x]){
                                if (y + i == FutY && x + i == FutX){
                                    return true;
                                }
                                break;
                            }else if (AnalyzedSideBoard[y + i, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y + i, AnalyzedIntBoard) == false && HasType(6, x + i, y + i, AnalyzedIntBoard) == false && HasType(3, x + i, y + i, AnalyzedIntBoard) == false){
                                if (y + i == FutY && x + i == FutX){
                                    return true;
                                }
                                break;
                            }
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + i >= 8 || x + (i * -1) <= -1){
                            break;
                        }
                        if (y + i <= 7 && x + (i * -1) >= 0 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                            if (y + i == FutY && x + (i * -1) == FutX){
                                return true;
                            }
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            if (y + i == FutY && x + (i * -1) == FutX){
                                return true;
                            }
                            break;
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(3, x + (i * -1), y + i, AnalyzedIntBoard) == false){
                            if (y + i == FutY && x + (i * -1) == FutX){
                                return true;
                            }
                            break;
                        }else {
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (x + i <= 7 || y + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                            if (y + (i * -1) == FutY && x + i == FutX){
                                return true;
                            }
                        }else if (y + (i * -1) <= 7 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != AnalyzedSideBoard[y, x]){
                            if (y + (i * -1) == FutY && x + i == FutX){
                                return true;
                            }
                            break;
                        }else if (y + (i * -1) <= 7 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y  + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + i, y + (i * -1), AnalyzedIntBoard) == false && HasType(3, x + i, y + (i * -1), AnalyzedIntBoard) == false){
                            if (y + (i * -1) == FutY && x + i == FutX){
                                return true;
                            }
                            break;
                        }else {
                            break;
                        }
                        
                    }
                    for (int i = 1; i <= 7; i++){
                        if (i > 7){
                            break;
                        }
                        if (y + (i * -1) >= 0 || x + (i * -1) >= 0){
                            break;
                        }
                        if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                            if (y + (i * -1) == FutY && x + (i * -1) ==FutX){
                                return true;
                            }
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            if (y + (i * -1) == FutY && x + (i * -1) ==FutX){
                                return true;
                            }
                            break;
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(3, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false){
                            if (y + (i * -1) == FutY && x + (i * -1) ==FutX){
                                return true;
                            }
                            break;
                        }else{
                            break;
                        }
                    }
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + i >= 8){
                        break;
                    }
                    if (y + i <= 7 && CanMoveHereData[y + i, x] == true && AnalyzedSideBoard[y + i, x] == 0){
                        if (y + i == FutY && x == FutX){
                            return true;
                        }
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != AnalyzedSideBoard[y, x]){
                        if (y + i == FutY && x == FutX){
                            return true;
                        }
                        break;
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + i, AnalyzedIntBoard) == false && HasType(6, x, y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x, 0], x, y, AnalyzedIntBoard) == false){
                        if (y + i == FutY && x == FutX){
                            return true;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess1");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (y + (i * -1) >= 7 || y + (i * -1) <= -1){
                        break;
                    }
                    if (y + (i * -1) >= 0 && CanMoveHereData[y + (i * -1), x] == true && AnalyzedSideBoard[y + (i * -1), x] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        if (y + (i * -1) == FutY && x == FutX){
                            return true;
                        }
                    }else {
                        if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + (i * -1), x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x, 0], x, y, AnalyzedIntBoard) == false)){
                            if (y + (i * -1) == FutY && x == FutX){
                                return true;
                            }
                            break;
                        }else{
                            break;
                        }
                    }
                }
                for (int i = 1; i <= 7; i ++){
                    if (i > 7){
                        break;
                    }
                    if (x + i > 7){
                        break;
                    }
                    if (x + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == 0 && HasType(4, x, y, AnalyzedIntBoard) == false){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                        break;
                    }else if (y + i <= 7 && CanMoveHereData[y, x + i] == true && AnalyzedSideBoard[y, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y, AnalyzedIntBoard) == false && HasType(6, x + i, y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + i, 0], x, y, AnalyzedIntBoard) == false){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess3");
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (x + (i * -1) <= -1){
                        break;
                    }
                    if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                        break;
                    }else if (x + (i * -1) >= 0 && CanMoveHereData[y, x + (i * -1)] == true && AnalyzedSideBoard[y, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                        break;
                    }else{
                        break;
                    }
                    
                }
                    //Debug.Log("Amazon: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Portal Thief 1 Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 8){
                if (y + 1 <= 7 && AnalyzedSideBoard[y, x] != AnalyzedSideBoard[y + 1, x]){
                    if (y + 1 == FutY && x == FutX){
                        return true;
                    }
                }else if (y + 1 <= 7 && AnalyzedSideBoard[y, x]== AnalyzedSideBoard[y + 1, x] && HasType(1, x, y + 1, AnalyzedIntBoard)){
                    if (y + 1 == FutY && x == FutX){
                        return true;
                    }
                }
                //Debug.Log("Portal Thief 1: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Portal Thief 2 Logic Code
            else if (AnalyzedIntBoard[y, x, m] == 9){
                if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] != Side){
                    if (y + 1 == FutY && x == FutX){
                        return true;
                    }
                }
                if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != Side){
                    if (y + 1 == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != Side){
                    if (y + 1 == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && AnalyzedSideBoard[y - 1, x] != Side){
                    if (y - 1 == FutY && x == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != Side){
                    if (y - 1 == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != Side){
                    if (y - 1 == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                if (x + 1 <= 7 && AnalyzedSideBoard[y, x + 1] != Side){
                    if (y == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (x - 1 >= 0 && AnalyzedSideBoard[y, x - 1] != Side){
                    if (y == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                //ebug.Log("Portal Thief 2: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Portal Thief 3 Logic Code:
            /*if (AnalyzedIntBoard[y, x, m] == 10){
                if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] != Side){
                    if (y + 1 == FutY && x == FutX){
                        return true;
                    }
                }
                if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != Side){
                    if (y + 1 == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != Side){
                    if (y + 1 == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && AnalyzedSideBoard[y - 1, x] != Side){
                    if (y - 1 == FutY && x == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != Side){
                    if (y - 1 == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != Side){
                    if (y - 1 == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                if (x + 1 <= 7 && AnalyzedSideBoard[y, x + 1] != Side){
                    if (y == FutY && x + 1 == FutX){
                        return true;
                    }
                }
                if (x - 1 >= 0 && AnalyzedSideBoard[y, x - 1] != Side){
                    if (y == FutY && x - 1 == FutX){
                        return true;
                    }
                }
                for (int i = -2; i <= 2; i++){
                    int num = 0;
                    num = x + i;
                    //Debug.Log("X: "+ num +" Y: "+ y + 2);
                    if (y + 2 <= 7 && num >= 0 && num <= 7 && AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x]){
                        if (y + 2 == FutY && )
                    }
                }
                for (int i = -2; i <= 2; i++){
                    int num2 = 0;
                    num2 = x + i;
                    //Debug.Log("X: "+ num2 +" Y: "+ y + 2);
                    if (y - 2 >= 0 && num2 >= 0 && num2 <= 7 && AnalyzedSideBoard[y - 2, x + i] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y - 2);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y - 2);
                        CanMoveHereData[y - 2, x + i] = false;
                    }
                }
                for (int i = -1; i <= 1; i++){
                    int num = 0;
                    num = y + i;
                    if (num <= 7 && num >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y + i, x - 2] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + i);
                        CanMoveHereData[y + i, x - 2] = false;
                    }
                }
                for (int i = -1; i <= 1; i++){
                    int num = 0;
                    num = y + i;
                    if (num <= 7 && num >= 0 && x + 2 >= 0 && AnalyzedSideBoard[y + i, x + 2] != AnalyzedSideBoard[y, x]){
                        CopyCurrentIntBoard();
                        CopyCurrentSideBoard();
                        MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + i);
                        EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + i);
                        CanMoveHereData[y + i, x + 2] = false;
                    }
                }
                Debug.Log("Portal Thief 3: Position of Checked Piece: ("+ x +", "+ y +")");
            }*/
            //Smiler Logic Code
            /*if (AnalyzedIntBoard[y, x, m] == 11){
                if (y + 1 <= 7 && x + 2 <= 7 && (AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    CanMoveHereData[y + 1, x + 2] = false;
                }
                if (y + 1 <= 7 && x - 2 <= 7 && (AnalyzedSideBoard[y + 1, x - 2] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    CanMoveHereData[y + 1, x - 2] = false;
                }
                if (y - 1 <= 7 && x - 2 <= 7 && (AnalyzedSideBoard[y - 1, x - 2] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y - 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    CanMoveHereData[y - 1, x - 2] = false;
                }
                if (y - 1 <= 7 && x + 2 <= 7 && (AnalyzedSideBoard[y - 1, x + 2] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y - 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    CanMoveHereData[y - 1, x + 2] = false;
                }
                if (y + 1 <= 7 && x + 2 <= 7 && (AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    CanMoveHereData[y + 1, x + 2] = false;
                }
                if (y + 2 <= 7 && x + 1 <= 7 && (AnalyzedSideBoard[y + 2, x + 1] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    CanMoveHereData[y + 2, x + 1] = false;
                }
                if (y + 2 <= 7 && x - 1 <= 7 && (AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y + 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    CanMoveHereData[y + 2, x - 1] = false;
                }
                if (y - 2 <= 7 && x + 1 <= 7 && (AnalyzedSideBoard[y - 2, x + 1] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y - 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    CanMoveHereData[y - 2, x + 1] = false;
                }
                if (y - 2 <= 7 && x - 1 <= 7 && (AnalyzedSideBoard[y - 2, x - 1] != AnalyzedSideBoard[y, x] || (AnalyzedSideBoard[y - 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(2, x, y, AnalyzedIntBoard) == false))){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    CanMoveHereData[y - 2, x - 1] = false;
                }
                if (y + 4 <= 7 && x + 2 <= 7 && AnalyzedSideBoard[y + 4, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 4);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 4);
                    CanMoveHereData[y + 4, x + 2] = false;
                }
                if (y + 4 <= 7 && x - 2 >= 0 && AnalyzedSideBoard[y + 4, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 4);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 4);
                    CanMoveHereData[y + 4, x - 2] = false;
                }
                if (y - 4 >= 0 && x + 2 <= 7 && AnalyzedSideBoard[y - 4, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 4);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 4);
                    CanMoveHereData[y - 4, x + 2] = false;
                }
                if (y - 4 >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y - 4, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 4);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 4);
                    CanMoveHereData[y - 4, x - 2] = false;
                }
                if (y + 2 <= 7 && x + 4 <= 7 && AnalyzedSideBoard[y + 2, x + 4] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y + 2);
                    CanMoveHereData[y + 2, x + 4] = false;
                }
                if (y - 2 >= 0 && x + 4 <= 7 && AnalyzedSideBoard[y - 2, x + 4] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y - 2);
                    CanMoveHereData[y - 2, x + 4] = false;
                }
                if (y + 2 <= 7 && x - 4 >= 0 && AnalyzedSideBoard[y + 2, x - 4] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y + 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y + 2);
                    CanMoveHereData[y + 2, x - 4] = false;
                }
                if (y - 2 >= 0 && x - 4 >= 0 && AnalyzedSideBoard[y - 2, x - 4] != AnalyzedSideBoard[y, x]){
                    CopyCurrentIntBoard();
                    CopyCurrentSideBoard();
                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y - 2);
                    EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y - 2);
                    CanMoveHereData[y - 2, x - 4] = false;
                }
                Debug.Log("Smiler: Position of Checked Piece: ("+ x +", "+ y +")");
            }else{
                //Debug.Log("No Correct Piece found Pos: (" + x +", "+ y +")");
            }*/
        }
        return false;
    }

    public bool HasType(int type, int x, int y, int[,,] AnalyzedIntBoard){
        for (int t = 0; t <= 15; t++){
            if (AnalyzedIntBoard[y, x, t] == type){
                return true;
            }
        }
        return false;
    }

    public int GetMaterial(int x, int y, int[,,] AnalyzedIntBoard){
        int Total = 0;
        for (int i = 0; i <= 15; i++){
            if (AnalyzedIntBoard[y, x, i] == 0){
                break;
            }else if (AnalyzedIntBoard[y, x, i] == 1){
                Total += 1;
            }else if (AnalyzedIntBoard[y, x, i] == 2){
                Total += 5;
            }else if (AnalyzedIntBoard[y, x, i] == 3){
                Total += 3;
            }else if (AnalyzedIntBoard[y, x, i] == 4){
                Total += 4;
            }else if (AnalyzedIntBoard[y, x, i] == 5){
                Total += 100;
            }else if (AnalyzedIntBoard[y, x, i] == 6){
                Total += 1000;
            }else if (AnalyzedIntBoard[y, x, i] == 7){
                Total += 9;
            }else if (AnalyzedIntBoard[y, x, i] == 8){
                Total += 3;
            }else if (AnalyzedIntBoard[y, x, i] == 9){
                Total += 7;
            }else if (AnalyzedIntBoard[y, x, i] == 10){
                Total += 5;
            }else if (AnalyzedIntBoard[y, x, i] == 11){
                Total += 8;
            }
        }
        if (Side == 1){
            return Total;
        }else if (Side == 2){
            return Total * -1;
        }else{
            return 0;
        }
    }

    //Evaluation left to code: Piece Defense and King or Queen Can be Captured 
    //AnalyzedIntBoard- Used IntBoard for evaluaation AnalyzedSideBoard- Used SideBoard for evaluation pastX & pastY- Refences past position of moved piece CurX & CurY- References current position of moved piece
    public float EvaluateBoard(int[,,] AnalyzedIntBoard, int[,] AnalyzedSideBoard, int pastX, int pastY, int CurX, int CurY){
        float SideChanger = 0.0f;
        if (Side == 1){
            SideChanger = 1f;
        }else if (Side == 2){
            SideChanger = -1f;
        }
        bool KingLost = true;
        bool QueenLost = true;
        bool EKingLost = true;
        bool EQueenLost = true;
        float Total = 0.0f;
        Total = 0f;
        Debug.Log("King Pos: "+ AnalyzedIntBoard[7, 4, 0]);
        Debug.Log("King Pos: "+ CurrentIntBoard[7, 4, 0]);
        for (int y = 7; y >= 0; y--){
            for (int x = 7; x >= 0; x--){
                if (AnalyzedIntBoard[y, x, 0] == 6 && AnalyzedSideBoard[y, x] == Side && KingLost == true){
                    Debug.Log("King Found on ("+ x +", "+ y +")");
                    KingLost = false;
                }
                if (AnalyzedIntBoard[y, x, 0] == 5 && AnalyzedSideBoard[y, x] == Side && QueenLost == true){
                    Debug.Log("Queen Found on ("+ x +", "+ y +")");
                    QueenLost = false;
                }
                if (AnalyzedIntBoard[y, x, 0] == 6 && AnalyzedSideBoard[y, x] != Side && EKingLost == true){
                    Debug.Log("Enemy King Found on ("+ x +", "+ y +")");
                    EKingLost = false;
                }
                if (AnalyzedIntBoard[y, x, 0] == 5 && AnalyzedSideBoard[y, x] != Side && EQueenLost == true){
                    Debug.Log("Enemy Queen Found on ("+ x +", "+ y +")");
                    EQueenLost = false;
                }
            }
        }
        if (QueenLost == true){
            Debug.Log("Queen Lost");
            Total += -999999999f * SideChanger;
        }
        if (KingLost == true){
            Debug.Log("King Lost");
            Total += -99999999999999f * SideChanger;
        }
        if (EQueenLost == true){
            Debug.Log("Enemy Queen Lost");
            Total += 999999999f * SideChanger;
        }
        if (EKingLost == true){
            Debug.Log("Enemy King Lost");
            Total += 99999999999999f * SideChanger;
        }
        /*for (int i = 0; i <= 15; i++){
            if (HasType(2, pastX, pastY, AnalyzedIntBoard) == true){
                Total += -100000000000 * SideChanger;
            }
        }*/
        /*if (QueenLost == true && KingLost == true && Side == 1){
            Total -= 99999999999999f * SideChanger;
        }else if (QueenLost == true && Side == 1){
            Total -= 999999999f * SideChanger;
        }
        else if (KingLost == true && Side == 1){
            Total -= 99999999999999f * SideChanger;
        }
        if (QueenLost == true && KingLost == true && Side == 2){
            Total -= -99999999999999f * SideChanger;
        }else if (QueenLost == true && Side == 2){
            Total -= -999999999f * SideChanger;
        }
        else if (KingLost == true && Side == 2){
            Total -= -99999999999999f * SideChanger;
        }*/
        CreateHeatMap(AnalyzedIntBoard, AnalyzedSideBoard, CheckingHeatBoard);
        if (Side == 2){
            CheckingHeatBoard[2, 3] = CheckingHeatBoard[2, 3] * CenterImportanceMult;
            CheckingHeatBoard[3, 3] = CheckingHeatBoard[3, 3] * CenterImportanceMult;
            CheckingHeatBoard[4, 3] = CheckingHeatBoard[4, 3] * CenterImportanceMult;
            CheckingHeatBoard[5, 3] = CheckingHeatBoard[5, 3] * CenterImportanceMult;
        }else if (Side == 1){
            CheckingHeatBoard[2, 4] = CheckingHeatBoard[2, 4] * CenterImportanceMult;
            CheckingHeatBoard[3, 4] = CheckingHeatBoard[3, 4] * CenterImportanceMult;
            CheckingHeatBoard[4, 4] = CheckingHeatBoard[4, 4] * CenterImportanceMult;
            CheckingHeatBoard[5, 4] = CheckingHeatBoard[5, 4] * CenterImportanceMult;
        }
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                Total += (float) CheckingHeatBoard[y, x];
            }
        }
        int EKingPosX = 0;
        int EKingPosY = 0;
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                if (AnalyzedIntBoard[y, x, 0] == 6 && AnalyzedSideBoard[y, x] != Side){
                    EKingPosX = x;
                    EKingPosY = y;
                }
            }
        }
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                //Debug.Log("Hi");
                if (AnalyzedIntBoard[y, x, 0] == 6 && Side == AnalyzedSideBoard[y, x]){
                    //Debug.Log("We're In");
                    KingPosX = x;
                    KingPosY = y;
                    break;
                }
                if (AnalyzedIntBoard[y, x, 0] == 5 && Side == AnalyzedSideBoard[y, x]){
                    //Debug.Log("We're In");
                    QueenPosX = x;
                    QueenPosY = y;
                    break;
                }
            }
        }
        if (CurX == 3 || CurX == 4 && CurY == 3 || CurY == 4){
            Total += 200 * SideChanger;
        }
        if (IsLegalCaptureMove(AnalyzedIntBoard, AnalyzedSideBoard, CurX, CurY, EKingPosX, EKingPosX) == true && (CheckingHeatBoard[CurX, CurY] <= -1 || CheckingHeatBoard[CurX, CurY] >= 1 || CheckingHeatBoard[CurX, CurY] == 0)){
            Total += 20 * SideChanger;
        }
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                if (CurrentSideBoard[y, x] != 0 && CurrentSideBoard[y, x] != Side && IsLegalCaptureMove(AnalyzedIntBoard, AnalyzedSideBoard, x, y, CurX, CurY) == true && GetMaterial(CurX, CurY, AnalyzedIntBoard) < GetMaterial(x, y, AnalyzedIntBoard)){
                    Total += 15 * SideChanger;
                }
                if (CurrentSideBoard[y, x] != 0 && CurrentSideBoard[y, x] != Side && IsLegalCaptureMove(AnalyzedIntBoard, AnalyzedSideBoard, CurX, CurY, x, y) == true && GetMaterial(CurX, CurY, AnalyzedIntBoard) > GetMaterial(x, y, AnalyzedIntBoard)){
                    Total -= 100 * SideChanger;
                }
            }
        }
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                if (IsLegalCaptureMove(AnalyzedIntBoard, AnalyzedSideBoard, x, y, KingPosX, KingPosY) == true){
                    Total += -9999999999999 * SideChanger;
                }
                if (IsLegalCaptureMove(AnalyzedIntBoard, AnalyzedSideBoard, x, y, QueenPosX, QueenPosY) == true){
                    Total += -9999999999999 * SideChanger;
                }
            }
        }
        //Total = Total / 64.0f;
        //Debug.Log("The Average Control is: "+ Total);
        int EqualControl = 0;
        int BlackControl = 0;
        int WhiteControl = 0;
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                if (CheckingHeatBoard[y, x] == 0){
                    EqualControl++;
                }else if (CheckingHeatBoard[y, x] >= 1){
                    WhiteControl++;
                }else if (CheckingHeatBoard[y, x] <= -1){
                    BlackControl--;
                }
            }
        }
        int SquareControlAmount = WhiteControl + BlackControl;
        SquareControlAmount = SquareControlAmount / 2;
        Total += SquareControlAmount * 100;
        Debug.Log("The Square Control Amount is: "+ SquareControlAmount);
        int MaterialAmount = BoardManagerObj.GetComponent<BoardManager>().GetAverageMaterial();
        Total += (float) MaterialAmount * 9999999999 * SideChanger;
        Debug.Log("The Material Score is: "+ MaterialAmount);
        //King Defense Evaluation: Will check if the King is defended but can escape when needed
        int LoopTimes = 0;
        float KingDefenseUp = 2.0f;
        float KingDefenseDown = 2.0f;
        float KingDefenseLeft = 2.0f;
        float KingDefenseRight = 2.0f;
        float flLoopTimes = 0.0f;
        //KingDefenseRight Evaluation
        for (int i = KingPosX; i >= 0; i--){
            if (i - 1 >= 0 && AnalyzedSideBoard[KingPosY, i - 1] == 0){
                LoopTimes++;
            }else{
                break;
            }
        }
        if (LoopTimes == 0){
            KingDefenseLeft = -1.0f;
        }else if (LoopTimes == 1){
            KingDefenseLeft = 0.0f;
        }else{
            flLoopTimes = (float) LoopTimes;
            KingDefenseLeft -= flLoopTimes * -1.0f;
        }
        //KingDefenseLeft Evaluation
        LoopTimes = 0;
        for (int i = KingPosX; i <= 7; i++){
            if (i + 1 <= 7 && AnalyzedSideBoard[KingPosY, i + 1] == 0){
                LoopTimes++;
            }else{
                break;
            }
        }
        if (LoopTimes == 0){
            KingDefenseRight = -1.0f;
        }else if (LoopTimes == 1){
            KingDefenseRight = 0.0f;
        }else{
            flLoopTimes = (float) LoopTimes;
            KingDefenseRight -= flLoopTimes * -1.0f;
        }
        //KingDefenseUp Evaluation
        LoopTimes = 0;
        for (int i = KingPosY; i <= 7; i++){
            //Debug.Log(i);
            if (i + 1 <= 7 && AnalyzedSideBoard[i + 1, KingPosX] == 0){
                LoopTimes++;
            }else{
                break;
            }
        }
        if (LoopTimes == 0){
            KingDefenseUp = -1.0f;
        }else if (LoopTimes == 1){
            KingDefenseUp = 0.0f;
        }else{
            flLoopTimes = (float) LoopTimes;
            KingDefenseUp -= flLoopTimes * -1.0f;
        }
        //KingDefenseDown
        LoopTimes = 0;
        for (int i = KingPosY; i >= 0; i--){
            if (i - 1 >= 0 && AnalyzedSideBoard[i - 1, KingPosX] == 0){
                LoopTimes++;
            }else{
                break;
            }
         }
        if (LoopTimes == 0){
            KingDefenseDown = -1.0f;
        }else if (LoopTimes == 1){
            KingDefenseDown = 0.0f;
        }else{
            flLoopTimes = (float) LoopTimes;
            KingDefenseDown -= flLoopTimes * -1.0f;
        }
        float KingDefenseTotal = KingDefenseUp + KingDefenseDown + KingDefenseLeft + KingDefenseRight;
        KingDefenseTotal = KingDefenseTotal / 4;
        Debug.Log("The king defense total is: "+ KingDefenseTotal);
        Total += KingDefenseTotal;
        if (Total > BestEvalScore && Side == 1){
            Debug.Log("Found Better Move Eval Score: "+ Total +" Would move from ("+ pastX +", "+ pastY +") to ("+ CurX +", "+ CurY +")");
            BestEvalScore = Total;
            ReferenceX = pastX;
            ReferenceY = pastY;
            PotentialX = CurX;
            PotentialY = CurY;
        }else if (Total < BestEvalScore && Side == 2){
            Debug.Log("Found Better Move. Eval Score: "+ Total +" Would move from ("+ pastX +", "+ pastY +") to ("+ CurX +", "+ CurY +")");
            BestEvalScore = Total;
            ReferenceX = pastX;
            ReferenceY = pastY;
            PotentialX = CurX;
            PotentialY = CurY;
        }else{
            Debug.Log("Didn't find a better move. Score: "+ Total +" Would move from ("+ pastX +", "+ pastY +") to ("+ CurX +", "+ CurY +")");
        }
        Debug.Log("-----------------------------------------------------");
        return Total;
    }

    public void MoveOnPlayerBoard(int x, int y, int dx, int dy){
        Debug.Log("Moved Piece on ("+ x +", "+ y +") to ("+ dx + ", "+ dy +")");
        GameObject Dot = BoardManagerObj.GetComponent<BoardManager>().GetDot(dy, dx);
        BoardManagerObj.GetComponent<BoardManager>().ChangeCurrentPiece(y, x);
        Debug.Log("Moved Piece on ("+ x +", "+ y +") to ("+ dx + ", "+ dy +")");
        BoardManagerObj.GetComponent<BoardManager>().MovePiece(Dot);
    }

    public void MovePiece(int[,,] ChangedIntBoard, int[,] ChangedSideBoard, int x, int y, int dx, int dy){
        if (ChangedSideBoard[y, x] == ChangedSideBoard[dy, dx]){
            for (int m = 0; m <= 15; m++){
                if (ChangedIntBoard[y, x, m] == 0){
                    ChangedIntBoard[y, x, m] = ChangedIntBoard[dy, dx, 0];
                    break;
                }
            }
        }
        for (int m = 0; m <= 15; m++){
            ChangedIntBoard[dy, dx, m] = ChangedIntBoard[y, x, m];
        }
        for (int c = 0; c <= 15; c++){
            ChangedIntBoard[y, x, c] = 0;
        }
        ChangedSideBoard[dy, dx] = ChangedSideBoard[y, x];
        ChangedSideBoard[y, x] = 0;
        if (HasType(2, dx, dy, ChangedIntBoard) == true || HasType(7, dx, dy, ChangedIntBoard) == true){
            Debug.Log("We're in");
            if (dx + 2 <= 7 && (x + 2 == dx && y + 1 == dy) || (x + 2 == dx && y - 1 == dy)){
                Debug.Log("X: "+ x +" Y: "+ y);
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y, x + 1, i] = 0;
                    ChangedIntBoard[y, x + 2, i] = 0;
                }
                ChangedSideBoard[y, x + 1] = 0;
                ChangedSideBoard[y, x + 2] = 0;
            }
            if ( x - 2 >= 0 && ((y + 1 <= 7 && x - 2 == dx && y + 1 == dy) || (y - 1 >= 0 && x - 2 == dx && y - 1 == dy))){
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y, x - 1, i] = 0;
                    ChangedIntBoard[y, x - 2, i] = 0;
                }
                ChangedSideBoard[y, x - 1] = 0;
                ChangedSideBoard[y, x - 2] = 0;
            }
            if (y + 2 <= 7 && (x + 1 == dx && y + 2 == dy) || (x - 1 == dx && y + 2 == dy)){
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y + 1, x, i] = 0;
                    ChangedIntBoard[y + 2, x, i] = 0;
                }
                ChangedSideBoard[y + 1, x] = 0;
                ChangedSideBoard[y + 2, x] = 0;
            }
            if ((x + 1 == dx && y - 2 == dy) || (x - 1 == dx && y - 2 == dy)){
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y - 1, x, i] = 0;
                    ChangedIntBoard[y - 2, x, i] = 0;
                }
                ChangedSideBoard[y - 1, x] = 0;
                ChangedSideBoard[y - 2, x] = 0;
            }
        }
    }

    public void FindBestMove(){
        if (Side == 1){
            BestEvalScore = -9999999999999999999999999999999f;
        }else if (Side == 2){
            BestEvalScore = 9999999999999999999999999999999f;
        }
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                for (int m = 0; m <= 15; m++){
                    //Debug.Log("Hi");
                    NextLegalMove(CurrentIntBoard, CurrentSideBoard, x, y, m);
                    ClearMoveHereData();
                    Debug.Log("========================================");
                }
            }
        }
    }

    public void ChooseRandomMove(){
        bool loop = true;
        float x = 0;
        float y = 0;
        int rx = 0;
        int ry = 0;
        while (loop == true){
            x = Random.Range(0.0f, 0.0f);
            y = Random.Range(0.0f, 0.0f);
            rx = (int) x;
            ry = (int) y;
            if (CurrentSideBoard[ry, rx] == Side){
                //MovePiece();
                loop = false;
            }
        }
    }

    void OnMouseDown(){
        ClearCurrentIntBoard();
        CreateBoardSnapshot();
        ClearCurrentHeatBoard(CurrentHeatBoard);
        CreateHeatMap(CurrentIntBoard, CurrentSideBoard, CurrentHeatBoard);
        //EvaluateBoard(CurrentIntBoard, CurrentSideBoard, 0, 0, 0, 0);
        FindBestMove();
        //Debug.Log("ReferenceX: "+ ReferenceX +"ReferenceY: ")
        MoveOnPlayerBoard(ReferenceX, ReferenceY, PotentialX, PotentialY);
        /*for (int i = 0; i < 7; i++){
            Debug.Log(CurrentHeatBoard[i, 0] + " " + CurrentHeatBoard[i, 1] + " " + CurrentHeatBoard[i, 2] + " " + CurrentHeatBoard[i, 3] + " " + CurrentHeatBoard[i, 4] + " " + CurrentHeatBoard[i, 5] + " " + CurrentHeatBoard[i, 6]);
        }*/
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}