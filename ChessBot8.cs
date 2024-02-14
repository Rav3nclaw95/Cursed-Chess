using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Any Debug.Log code is used for debugging and seeing any info that can't be seen when the preview of the game is running
/*
    Piece values:
    1 = Pawn
    2 = Knight
    3 = Bishop
    4 = Rook
    5 = Queen
    6 = King
    7 = Amazon (Queen + Knight combined)
    8 = Scout lvl 1
    9 = Scout lvl 2
    10 = Scout lvl 3
    11 = Smiler
*/
public class ChessBot8 : MonoBehaviour
{
    [SerializeField] bool VeryDrunkMode;
    [SerializeField] float CenterImportanceMult;
    [SerializeField] float MaterialMultiplier;
    [SerializeField] float SpecialPeiceMoveReward;
    [SerializeField] float[] PawnMaterialBoard = new float[64];
    [SerializeField] float[] KnightMaterialBoard = new float[64];
    [SerializeField] float[] BishopMaterialBoard = new float[64];
    [SerializeField] float[] RookMaterialBoard = new float[64];
    [SerializeField] float[] QueenMaterialBoard = new float[64];
    [SerializeField] float[] KingMaterialBoard = new float[64];
    [SerializeField] int MaxDepth;
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
    private GameObject[,] CurrentObjBoard = new GameObject[8, 8];
    private int[,,] CurrentIntBoard = new int[8, 8, 16];
    private int[,] CurrentHeatBoard = new int[8, 8];
    [SerializeField] int[,] CurrentSideBoard = new int[8, 8];
    private int[,,] CheckingIntBoard = new int[8, 8, 16];
    private int[,] CheckingHeatBoard = new int[8, 8];
    private int[,] CheckingSideBoard = new int[8, 8];
    [SerializeField] int BoardMoveCount;
    [SerializeField] float[] MoveData = new float[3];
    [SerializeField] bool[,] CanMoveHereData = new bool[7, 7];

    //Sets the board that the bot is playing on
    public void SetBoard(GameObject Board){
        BoardManagerObj = Board;
    }

    //Sets what side the bot is playing on: 1=White 2=Black
    public void SetSide(int side){
        Side = side;
    }

    //Copies the 1st IntBoard onto the 2nd IntBoard
    public void CopyIntBoard(int[,,] CopiedIntBoard, int[,,] CopyIntBoard){
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                for (int m = 0; m <= 15; m++){
                    CopyIntBoard[y, x, m] = CopiedIntBoard[y, x, m];
                }
            }
        }
    }

    //Clears the given IntBoard
    public void ClearIntBoard(int[,,] ClearedIntBoard){
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                for (int m = 0; m <= 7; m++){
                    ClearedIntBoard[y, x, m] = 0;
                }
            }
        }
    }

    //Clears the given SideBoard
    public void ClearSideBoard(int[,] ClearedSideBoard){
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                ClearedSideBoard[y, x] = 0;
            }
        }
    }

    public void ClearHeatBoard(int[,] GivenHeatBoard){
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                GivenHeatBoard[y, x] = 0;
            }
        }
    }

    //Copies the 1st SideBoard onto the 2nd SideBoard
    public void CopySideBoard(int[,] CopiedSideBoard, int[,] CopySideBoard){
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                CopySideBoard[y, x] = CopiedSideBoard[y, x];
            }
        }
    }

    //Clears the MoveHereData list that's used to stop running moves that were already checked
    public void ClearMoveHereData(){
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                CanMoveHereData[y, x] = true;
            }
        }
    }

    public GameObject GetUsedBoard(){
        return BoardManagerObj;
    }

    public int GetSide(){
        return Side;
    }

    //Returns the amount of material that the piece on the given coordinates is worth
    public float GetMaterial(int x, int y, int[,,] AnalyzedIntBoard, int CheckedSide){
        float Total = 0f;
        for (int i = 0; i <= 15; i++){
            if (AnalyzedIntBoard[y, x, i] == 0){
                Total += 0;
            }else if (AnalyzedIntBoard[y, x, i] == 1){
                if (Side == 2){
                    Total = PawnMaterialBoard[x * (7 - y)];
                }else{
                    Total = PawnMaterialBoard[x * y];
                }
            }else if (AnalyzedIntBoard[y, x, i] == 2){
                if (Side == 2){
                    Total = KnightMaterialBoard[x * (7 - y)];
                }else{
                    Total = KnightMaterialBoard[x * y];
                }
            }else if (AnalyzedIntBoard[y, x, i] == 3){
                if (Side == 2){
                    Total = BishopMaterialBoard[x * (7 - y)];
                }else{
                    Total = BishopMaterialBoard[x * y];
                }
            }else if (AnalyzedIntBoard[y, x, i] == 4){
                if (Side == 2){
                    Total = RookMaterialBoard[x * (7 - y)];
                }else{
                    Total = RookMaterialBoard[x * y];
                }
            }else if (AnalyzedIntBoard[y, x, i] == 5){
                if (Side == 2){
                    Total = QueenMaterialBoard[x * (7 - y)];
                }else{
                    Total = QueenMaterialBoard[x * y];
                }
            }else if (AnalyzedIntBoard[y, x, i] == 6){
                if (Side == 2){
                    Total = KingMaterialBoard[x * (7 - y)];
                }else{
                    Total = KingMaterialBoard[x * y];
                }
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
        if (CheckedSide == 1){
            return Total;
        }else if (CheckedSide == 2){
            return Total * -1;
        }else{
            return 0;
        }
    }

    //Checks if the piece on the give coordinates has the given type and if found returns true. If not found it returns false
    public bool HasType(int type, int x, int y, int[,,] AnalyzedIntBoard){
        for (int t = 0; t <= 15; t++){
            if (AnalyzedIntBoard[y, x, t] == type){
                return true;
            }
        }
        return false;
    }

    public float[] RandomizeMaterialBoard(float[] RandomBoard){
        for (int i = 0; i <= 63; i++){
            RandomBoard[i] = Random.Range(-10f, 10f);
        }
        return RandomBoard;
    }

    /*
    Planned eval: Finds own king & queen & enemy king & queen. Once done if bool values for each are true it'll add or subract from it's total score
    Eval will get a heat board
    */
    //Evaluates the given position and returns the evaluation assigned to the position
    public float EvaluateBoard(int[,,] AnalyzedIntBoard, int[,] AnalyzedSideBoard, int pastX, int pastY, int CurX, int CurY){
        Debug.Log("Side given to Evaluation: "+ Side);
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
        int EKingPosX = 0;
        int EKingPosY = 0;
        float Total = 0.0f;
        float MaterialTotal = 0.0f;
        float AverageControl = 0f;
        float WhiteControl = 0f;
        float BlackControl = 0f;
        CreateHeatMap(AnalyzedIntBoard, AnalyzedSideBoard, CheckingHeatBoard);
        if (Side == 1){
            CheckingHeatBoard[3, 3] = CheckingHeatBoard[3, 3] * ((int) CenterImportanceMult * (int) SideChanger);
            CheckingHeatBoard[3, 4] = CheckingHeatBoard[3, 4] * ((int) CenterImportanceMult * (int) SideChanger);
        }else if (Side == 2){
            CheckingHeatBoard[4, 3] = CheckingHeatBoard[4, 3] * ((int) CenterImportanceMult * (int) SideChanger);
            CheckingHeatBoard[4, 4] = CheckingHeatBoard[4, 4] * ((int) CenterImportanceMult * (int) SideChanger);
        }
        if (AnalyzedIntBoard[CurY, CurX, 0] == 6 && AnalyzedSideBoard[CurY, CurX] == Side){
            KingLost = false;
            KingPosX = pastY;
            KingPosY = pastX;
        }else if (AnalyzedIntBoard[CurY, CurX, 0] == 5 && AnalyzedSideBoard[CurY, CurX] == Side){
            QueenLost = false;
            QueenPosX = pastX;
            QueenPosY = pastY;
        }
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                //Debug.Log("KingFind Current Cords: ("+ x +", "+ y +") Side of Piece: "+ AnalyzedSideBoard[y, x] +" Piece: "+ AnalyzedIntBoard[y, x, 0]);
                if (KingLost == true && AnalyzedIntBoard[y, x, 0] == 6 && AnalyzedSideBoard[y, x] == Side){
                    //Debug.Log("King Found on ("+ x +", "+ y +")");
                    KingLost = false;
                    KingPosX = x;
                    KingPosY = y;
                }
                if (QueenLost == true && AnalyzedIntBoard[y, x, 0] == 5 && AnalyzedSideBoard[y, x] == Side){
                    //Debug.Log("Queen Found on ("+ x +", "+ y +")");
                    QueenLost = false;
                    QueenPosX = x;
                    QueenPosY = y;
                }else if (EKingLost == true && AnalyzedIntBoard[y, x, 0] == 6 && AnalyzedSideBoard[y, x] != Side){
                    //Debug.Log("Enemy King Found on ("+ x +", "+ y +")");
                    EKingLost = false;
                    EKingPosX = x;
                    EKingPosY = y;
                }else if (EQueenLost == true && AnalyzedIntBoard[y, x, 0] == 5 && AnalyzedSideBoard[y, x] != Side){
                    //Debug.Log("Enemy Queen Found on ("+ x +", "+ y +")");
                    EQueenLost = false;
                }
                AverageControl += CheckingHeatBoard[y, x];
                if (CheckingHeatBoard[y, x] > 0){
                    WhiteControl++;
                }else if (CheckingHeatBoard[y, x] < 0){
                    BlackControl--;
                }
                if (AnalyzedSideBoard[y, x] == 1){
                    MaterialTotal += GetMaterial(x, y, AnalyzedIntBoard, 1);
                }else if (AnalyzedSideBoard[y, x] == 2){
                    MaterialTotal += GetMaterial(x, y, AnalyzedIntBoard, 2);
                }
            }
        }
        if (AnalyzedIntBoard[CurY, CurX, 0] != 1){
            Total += SpecialPeiceMoveReward * SideChanger;
        }
        Total += MaterialTotal * (MaterialMultiplier * SideChanger);
        if (QueenLost == true){
            Debug.Log("Queen Lost");
            Total += -999999999f * SideChanger;
        }
        if (KingLost == true){
            Debug.Log("King Lost");
            Total += -99999999999999f * SideChanger;
        }
        if (EQueenLost == true){
            //Debug.Log("Enemy Queen Lost");
            Total += 999999999999999f * SideChanger;
        }
        if (EKingLost == true){
            //Debug.Log("Enemy King Lost");
            Total += 999999999999999999f * SideChanger;
        }
        float PieceControl = 0f;
        PieceControl = WhiteControl + BlackControl;
        PieceControl = PieceControl / 64;
        Total += PieceControl;
        AverageControl = AverageControl / 64;
        Total += AverageControl;
        Debug.Log("The Total evaluation is: "+ Total +" At a pos of: ("+ pastX +", "+ pastY +") to the position ("+ CurX +", "+ CurY +")");
        return Total;
    }

    //Moves the chosen piece on x & y to the position dx & dy on the actual board being played on
    public void MoveOnPlayerBoard(int x, int y, int dx, int dy){
        Debug.Log("Moved Piece on ("+ x +", "+ y +") to ("+ dx + ", "+ dy +")");
        GameObject Dot = BoardManagerObj.GetComponent<BoardManager>().GetDot(dy, dx);
        BoardManagerObj.GetComponent<BoardManager>().ChangeCurrentPiece(y, x);
        BoardManagerObj.GetComponent<BoardManager>().MovePiece(Dot);
    }

    //Moves the chosen piece on the given sideboard and returns it
    public int[,] MoveSidePiece(int[,,] ChangedIntBoard, int[,] ChangedSideBoard, int dx, int dy, int x, int y){
        ChangedSideBoard[y, x] = ChangedSideBoard[dy, dx];
        //Debug.Log("An: "+ ChangedSideBoard[dy, dx]);
        //Debug.Log("Bn: "+ ChangedSideBoard[y, x]);
        //Debug.Log("d Cords: ("+ dx +", "+ dy +")");
        //Debug.Log("Normal Cords: ("+ x +", "+ y +")");
        ChangedSideBoard[y, x] = 0;
        if (HasType(2, dx, dy, ChangedIntBoard) == true || HasType(7, dx, dy, ChangedIntBoard) == true){
            if (dx + 2 <= 7 && (x + 2 == dx && y + 1 == dy) || (x + 2 == dx && y - 1 == dy)){
                ChangedSideBoard[y, x + 1] = 0;
                ChangedSideBoard[y, x + 2] = 0;
            }
            if ( x - 2 >= 0 && ((y + 1 <= 7 && x - 2 == dx && y + 1 == dy) || (y - 1 >= 0 && x - 2 == dx && y - 1 == dy))){
                ChangedSideBoard[y, x - 1] = 0;
                ChangedSideBoard[y, x - 2] = 0;
            }
            if (y + 2 <= 7 && (x + 1 == dx && y + 2 == dy) || (x - 1 == dx && y + 2 == dy)){
                ChangedSideBoard[y + 1, x] = 0;
                ChangedSideBoard[y + 2, x] = 0;
            }
            if ((x + 1 == dx && y - 2 == dy) || (x - 1 == dx && y - 2 == dy)){
                ChangedSideBoard[y - 1, x] = 0;
                ChangedSideBoard[y - 2, x] = 0;
            }
        }
        return ChangedSideBoard;
    }

    //Moves the chosen piece on the given intboard and returns the new board. 
    public int[,,] MoveIntPiece(int[,,] ChangedIntBoard, int[,] ChangedSideBoard, int x, int y, int dx, int dy){
        //Debug.Log("MoveIntPiece given cords("+ x +", "+ y +") to ("+ dx +", "+ dy +")");
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
        if (HasType(2, dx, dy, ChangedIntBoard) == true || HasType(7, dx, dy, ChangedIntBoard) == true){
            Debug.Log("We're in for knight");
            if (x + 2 <= 7 && (x + 2 == dx && y + 1 == dy) || (x + 2 == dx && y - 1 == dy)){
                Debug.Log("X: "+ x +" Y: "+ y);
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y, x + 1, i] = 0;
                    ChangedIntBoard[y, x + 2, i] = 0;
                }
            }else if ( x - 2 >= 0 && ((y + 1 <= 7 && x - 2 == dx && y + 1 == dy) || (y - 1 >= 0 && x - 2 == dx && y - 1 == dy))){
                Debug.Log("X2: "+ x +" Y: "+ y);
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y, x - 1, i] = 0;
                    ChangedIntBoard[y, x - 2, i] = 0;
                }
            }else if (y + 2 <= 7 && (x + 1 == dx && y + 2 == dy) || (x - 1 == dx && y + 2 == dy)){
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y + 1, x, i] = 0;
                    ChangedIntBoard[y + 2, x, i] = 0;
                }
            }else if ((x + 1 == dx && y - 2 == dy) || (x - 1 == dx && y - 2 == dy)){
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y - 1, x, i] = 0;
                    ChangedIntBoard[y - 2, x, i] = 0;
                }
            }
        }
        return ChangedIntBoard;
    }

    //Sets CurrentIntBoard with current board data and sets CurrentSideBoard with each side is
    public void CreateBoardSnapshot(){
        CurrentObjBoard = BoardManagerObj.GetComponent<BoardManager>().GetBoard();
        ClearIntBoard(CurrentIntBoard);
        ClearSideBoard(CurrentSideBoard);
        bool Side1 = false;
        //Fills CurrentIntBoard with number data of each piece
        for (int i = 7; i >= 0; i--){
            for (int t = 7; t >= 0; t--)
            {
                if (CurrentObjBoard[i, t] != null){
                    Side1 = BoardManagerObj.GetComponent<BoardManager>().GetPieceSide(t, i);
                }else{
                    CurrentSideBoard[i, t] = 0;
                }
                if (Side1 == true && CurrentObjBoard[i, t] != null){
                    CurrentSideBoard[i, t] = 1;
                    //Debug.Log("1 Created at ("+ t +", "+ i +")");
                }else if (Side1 == false && CurrentObjBoard[i, t] != null){
                    CurrentSideBoard[i, t] = 2;
                    //Debug.Log("2 Created");
                }else if (CurrentObjBoard[i, t] != null){
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
                //Debug.Log("Current created square: ("+ t +", "+ i +") Piece: "+ CurrentIntBoard[i, t, 0] +" Side: "+ CurrentSideBoard[i, t]);
            }
        }
        Debug.Log("Board Created");
        /*for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                Debug.Log(CurrentSideBoard[y, x]);
            }
        }*/
        //Debug.Log(CurrentObjBoard[1, 3]);
    }

    //Creates a CurrentHeatMap using the CurrentIntBoard 3D List
    public void CreateHeatMap(int[,,] AnalyzedIntBoard, int[,] AnalyzedSideBoard, int[,] OutputHeatBoard){
        ClearHeatBoard(OutputHeatBoard);
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
                    //Use for debugging to check if HeatBoard is working correctly on specific coordinates
                    //Debug.Log("Piece: "+ AnalyzedIntBoard[y, x, n] +" Side: "+ AnalyzedSideBoard[y, x] + " Pos: ("+ x +", "+ y +") Pawn Pos: "+ OutputHeatBoard[1, 1]);
                }
            }
        }
        //Used for showing heat board on the console
        /*for (int i = 0; i <= 7; i++){
            Debug.Log(OutputHeatBoard[i, 0] + " " + OutputHeatBoard[i, 1] + " " + OutputHeatBoard[i, 2] + " " + OutputHeatBoard[i, 3] + " " + OutputHeatBoard[i, 4] + " " + OutputHeatBoard[i, 5] + " " + OutputHeatBoard[i, 6] + " " + OutputHeatBoard[i, 7]);
        }*/
    }

    //Finds the best possible move for that specific type and returns the position of the piece and where it'll move
    public float[] NextLegalMove(int[,,] AnalyzedIntBoard, int[,] AnalyzedSideBoard, int x, int y, int m, int GivenSide){
        /* Documentation:
            Paramaters: 
            AnalyzedIntBoard- Board given to be found the best move on that holds all pieces that current postition holds
            AnalyzedSideBoard- Board given to be found the best move on that holds the sides of all pieces
            x- X position of the piece you're trying to find the best move for on the board
            y- Y position of the piece you're trying to find the best move for on the board
            m- M position for what piece type it's finding the best move for
            GivenSide- Not used anywhere currently
            Return variable:
            A 3 element float array that holds the following values in each position:
            0- The evaluation for the move
            1- The X coordinate where you move the piece
            2- The Y coordinate where you move the piece
        */
        Debug.Log("Running a new piece search");
        Debug.Log("Current Side: "+ Side);
        //Debug.Log("PreKingMove: "+ AnalyzedSideBoard[6, 5]);
        int[,,] UsedIntBoard = new int[8, 8, 16];
        int[,] UsedSideBoard = new int[8, 8];
        float[] SentData = new float[3];
        if (Side == 1){
            SentData[0] = -9999999999999999999999f;
        }else{
            SentData[0] = 9999999999999999999999f;
        }
        float Evaluation = 0f;
            if  (AnalyzedSideBoard[y, x] == Side && AnalyzedIntBoard[y, x, m] != 0){
            //Knight Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 2){
                if (y + 1 <= 7 && x + 2 <= 7 && AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, UsedIntBoard);
                    CopySideBoard(AnalyzedSideBoard, UsedSideBoard);
                    UsedIntBoard = MoveIntPiece(UsedIntBoard, UsedSideBoard, x, y, x + 2, y + 1);
                    UsedSideBoard = MoveSidePiece(UsedIntBoard, UsedSideBoard, x, y, x + 2, y + 1);
                    Evaluation = EvaluateBoard(UsedIntBoard, UsedSideBoard, x, y, x + 2, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[1] = y + 1;
                        SentData[2] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 2;
                    }
                }else if (y + 1 <= 7 && x + 2 <= 7 && AnalyzedSideBoard[y + 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y + 1, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, UsedIntBoard);
                    CopySideBoard(AnalyzedSideBoard, UsedSideBoard);
                    UsedIntBoard = MoveIntPiece(UsedIntBoard, UsedSideBoard, x, y, x + 2, y + 1);
                    UsedSideBoard = MoveSidePiece(UsedIntBoard, UsedSideBoard, x, y, x + 2, y + 1);
                    Evaluation = EvaluateBoard(UsedIntBoard, UsedSideBoard, x, y, x + 2, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[1] = y + 1;
                        SentData[2] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 2;
                    }
                }
                if (y + 1 <= 7 && x - 2 >= 0 && AnalyzedSideBoard[y + 1, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, UsedIntBoard);
                    CopySideBoard(AnalyzedSideBoard, UsedSideBoard);
                    UsedIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    UsedSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 2;
                    }
                }else if (y + 1 <= 7 && x - 2 >= 0 && AnalyzedSideBoard[y + 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y + 1, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    AnalyzedIntBoard = MoveIntPiece(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x - 2, y + 1);
                    AnalyzedSideBoard = MoveSidePiece(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x - 2, y + 1);
                    Evaluation = EvaluateBoard(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x - 2, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 2;
                    }
                }
                if (y - 1 >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y - 1, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 2;
                    }
                }else if (y - 1 >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y - 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y - 1, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 2;
                    }
                }
                if (y - 1 >= 0 && x + 2 <= 7 && AnalyzedSideBoard[y - 1, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 2;
                    }
                }else if (y - 1 >= 0 && x + 2 <= 7 && AnalyzedSideBoard[y - 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y - 1, AnalyzedIntBoard) == false){
                    CopyIntBoard(CurrentIntBoard, CheckingIntBoard);
                    CopySideBoard(CurrentSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 2;
                    }
                }
                if (y + 2 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 2, x + 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 1;
                    }
                }else if (y + 2 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y + 2, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 1;
                    }
                }
                if (y + 2 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 1;
                    }
                }else if (y + 2 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y + 2, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 1;
                    }
                }
                if (y - 2 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 2, x + 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 1;
                    }
                }else if (y - 2 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y - 2, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 1;
                    }
                }
                if (y - 2 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 2, x - 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 1;
                    }
                }else if (y - 2 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y - 2, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 1;
                    }
                }
                Debug.Log("Knight: Position of Checked Piece: ("+ x +", "+ y +") Sent Data to pos: ("+ SentData[1] +", "+ SentData[2] +")");
                return SentData;
            }
            //Bishop Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 3){
                    for (int i = 1; i <= 7; i++){
                        if (y + i <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + i] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CanMoveHereData[y + i, x + i] = false;
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }
                        }else if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }
                            break;
                        }else if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y + i, AnalyzedIntBoard) == false && HasType(6, x + i, y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x + i, 0], x, y, AnalyzedIntBoard) == false){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }
                            break;
                        }else{
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (y + i <= 7 && x + (i * -1) >= 0 &&  AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }
                            break;
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }
                            break;
                        }else {
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }
                        }else if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }
                            break;
                        }else if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y  + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + i, y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x + i, 0], x, y, AnalyzedIntBoard) == false){
                            Debug.Log("hello");
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }
                            break;
                        }else {
                            break;
                        }
                        
                    }
                    for (int i = 1; i <= 7; i++){
                        if (y + (i * -1) >= 0 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }
                        }else if (y + (i * -1) >= 0 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }
                            break;
                        }else if (y + (i * -1) >= 0 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }
                            break;
                        }else{
                            break;
                        }
                    }
                    Debug.Log("Bishop: Position of Checked Piece: ("+ x +", "+ y +")");
                    return SentData;
            }
            //Rook Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 4){
                for (int i = 1; i <= 7; i++){
                    if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }
                        break;
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + i, AnalyzedIntBoard) == false && HasType(6, x, y + i, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + i, x, 0], x, y, AnalyzedIntBoard) == false){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess1");
                for (int i = 1; i <= 7; i++){
                    if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }
                    }else if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }
                        break;
                    }else if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] == AnalyzedSideBoard[y, x] && HasType(5, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x, y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x, 0], x, y, AnalyzedIntBoard) == false){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                for (int i = 1; i <= 7; i++){
                    if (x + i <= 7 && AnalyzedSideBoard[y, x + i] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }
                    }else if (x + i <= 7 && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }
                        break;
                    }else if (x + i <= 7 && AnalyzedSideBoard[y, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y, AnalyzedIntBoard) == false && HasType(6, x + i, y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + i, 0], x, y, AnalyzedIntBoard) == false){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess3");
                for (int i = 1; i <= 7; i++){
                    if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }
                    }else if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }
                        break;
                    }else if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }
                        break;
                    }else{
                        break;
                    }
                    
                }
                Debug.Log("Rook: Position of Checked Piece: ("+ x +", "+ y +")");
                return SentData;
            }
            //Pawn Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 1){
                if (AnalyzedSideBoard[y, x] == 1){
                    if (y == 1){
                        if (AnalyzedSideBoard[y + 2, x] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 2);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 2);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 2);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + 2;
                                SentData[1] = x;
                            }
                        }
                    }
                    if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + 1;
                            SentData[1] = x;
                        }
                    }
                    if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != 0 && HasType(AnalyzedIntBoard[y + 1, x + 1, 0], x, y, AnalyzedIntBoard) == false){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + 1;
                            SentData[1] = x + 1;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + 1;
                            SentData[1] = x + 1;
                        }
                    }
                    if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != 0 && HasType(AnalyzedIntBoard[y + 1, x - 1, 0], x, y, AnalyzedIntBoard) == false){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + 1;
                            SentData[1] = x - 1;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + 1;
                            SentData[1] = x - 1;
                        }
                    }
                    Debug.Log("Ran White Pawn Code");
                    return SentData;
                }
                if (AnalyzedSideBoard[y, x] == 2){
                    if (y == 6){
                       if (AnalyzedSideBoard[y - 2, x] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 2);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 2);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 2);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y - 2;
                                SentData[1] = x;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y - 2;
                                SentData[1] = x;
                            }
                        }
                    }
                    if (y - 1 >= 0 && AnalyzedIntBoard[y - 1, x, 0] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y - 1;
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y - 1;
                            SentData[1] = x;
                        }
                    }
                    if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != 0 && HasType(AnalyzedIntBoard[y - 1, x + 1, 0], x, y, AnalyzedIntBoard) == false){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y - 1;
                            SentData[1] = x + 1;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y - 1;
                            SentData[1] = x + 1;
                        }
                    }
                    if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != 0 && HasType(AnalyzedIntBoard[y - 1, x - 1, 0], x, y, AnalyzedIntBoard) == false){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y - 1;
                            SentData[1] = x - 1;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y - 1;
                            SentData[1] = x - 1;
                        }
                    }
                    Debug.Log("Ran Black Pawn Code");
                    return SentData;
                }
                Debug.Log("Pawn: Position of Checked Piece: ("+ x +", "+ y +")");
            }
            //Queen Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 5){
                //Diagonal Logic code
                    bool loop = true;
                    int i = 0;
                    while (loop == true){
                        i++;
                        if (y + i <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + i] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CanMoveHereData[y + i, x + i] = false;
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }
                        }else if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }
                            loop = false;
                        }else{
                            loop = false;
                        }
                    }
                    i = 0;
                    loop = true;
                    while (loop == true){
                        i++;
                        if (y + i <= 7 && x + (i * -1) >= 0 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }
                            loop = false;
                        }else {
                            loop = false;
                        }
                    }
                    i = 0;
                    loop = true;
                    while (loop == true){
                        i++;
                        if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }
                        }else if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }
                            loop = false;
                        }else {
                            loop = false;
                        }
                        
                    }
                    i = 0;
                    loop = true;
                    while (loop == true){
                        i++;
                        if (y + (i * -1) >= 0 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }
                        }else if (y + (i * -1) >= 0 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }
                            loop = false;
                        }else{
                            loop = false;
                        }
                    }
                loop = true;
                i = 0;
                while (loop == true){
                    i++;
                    /*if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == 0){
                        Debug.Log("We're in");
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        AnalyzedIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        AnalyzedSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        Evaluation = EvaluateBoard(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x, y + i);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }
                    }else */if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != AnalyzedSideBoard[y, x]){
                        Debug.Log("We're in");
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        AnalyzedIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        AnalyzedSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        Evaluation = EvaluateBoard(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x, y + i);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }
                        loop = false;
                    }else{
                        loop = false;
                    }
                }
                //Debug.Log("Sucess1");
                i = 0;
                loop = true;
                while (loop == true){
                    i++;
                    if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        AnalyzedIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        AnalyzedSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        Evaluation = EvaluateBoard(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x, y + (i * -1));
                        CanMoveHereData[y + (i * -1), x] = false;
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }
                    }else if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        AnalyzedIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        AnalyzedSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        Evaluation = EvaluateBoard(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x, y + (i * -1));
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }
                        loop = false;
                    }else{
                        loop = false;
                    }
                }
                i = 0;
                loop = true;
                while (loop == true){
                    i++;
                    if (x + i <= 7 && AnalyzedSideBoard[y, x + i] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        AnalyzedIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        AnalyzedSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        Evaluation = EvaluateBoard(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x + i, y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }
                    }else if (x + i <= 7 && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        AnalyzedIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        AnalyzedSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        Evaluation = EvaluateBoard(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x + i, y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }
                        loop = false;
                    }else{
                        loop = false;
                    }
                }
                //Debug.Log("Sucess3");
                i = 0;
                loop = true;
                while (loop == true){
                    i++;
                    if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        AnalyzedIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        AnalyzedSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        Evaluation = EvaluateBoard(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x + (i * -1), y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }
                    }else if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        AnalyzedIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        AnalyzedSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        Evaluation = EvaluateBoard(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x + (i * -1), y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }
                        break;
                    }else{
                        break;
                    }
                    
                }
                Debug.Log("Queen: Position of checked piece: ("+ x +", "+ y +")");
            }
            //King Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 6){
                Debug.Log("PostKingMove: "+ AnalyzedSideBoard[6, 5]);
                if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x;
                    }
                }
                if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 1;
                    }
                }
                if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 1;
                    }
                }
                if (y - 1 >= 0 && AnalyzedSideBoard[y - 1, x] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x;
                    }
                }
                if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 1;
                    }
                }
                if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 1;
                    }
                }
                if (x + 1 <= 7 && AnalyzedSideBoard[y, x + 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x + 1;
                    }
                }
                if (x - 1 >= 0 && AnalyzedSideBoard[y, x - 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x - 1;
                    }
                }
                Debug.Log("King: Position of Checked Piece: ("+ x +", "+ y +")");
                return SentData;
            }
            //Amazon Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 7){
                if (y + 1 <= 7 && x + 2 <= 7 && AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    MoveIntPiece(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x + 2, y + 1);
                    MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 2;
                    }
                }
                if (y + 1 <= 7 && x - 2 >= 0 && AnalyzedSideBoard[y + 1, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 2;
                    }
                }
                if (y - 1 >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y - 1, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 2;
                    }
                }
                if (y - 1 >= 0 && x + 2 <= 7 && AnalyzedSideBoard[y - 1, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 2;
                    }
                }
                if (y + 2 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 2, x + 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 1;
                    }
                }
                if (y + 2 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 1;
                    }
                }
                if (y - 2 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 2, x + 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 1;
                    }
                }
                if (y - 2 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 2, x - 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 1;
                    }
                }
                //Diagonal Logic code
                    for (int i = 1; i <= 7; i++){
                        if (y + i <= 7 && x + i <= 7 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + i] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CanMoveHereData[y + i, x + i] = false;
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }
                        }else if (y + i <= 7 && x + i <= 7 && AnalyzedSideBoard[y + i, x + i] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + i;
                            }
                            break;
                        }else{
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (y + i <= 7 && x + (i * -1) >= 0 && CanMoveHereData[y, x] == true && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }
                        }else if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + i);
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + i;
                                SentData[1] = x + (i * -1);
                            }
                            break;
                        }else {
                            break;
                        }
                    }
                    for (int i = 1; i <= 7; i++){
                        if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }
                        }else if (y + (i * -1) >= 0 && x + i <= 7 && AnalyzedSideBoard[y + (i * -1), x + i] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + i;
                            }
                            break;
                        }else {
                            break;
                        }
                        
                    }
                    for (int i = 1; i <= 7; i++){
                        if (y + (i * -1) >= 0 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == 0){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }
                        }else if (y + (i * -1) >= 0 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
                            CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                            CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                            CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y + (i * -1));
                            if (Evaluation > SentData[0] && Side == 1){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }else if (Evaluation < SentData[0] && Side == 2){
                                SentData[0] = Evaluation;
                                SentData[2] = y + (i * -1);
                                SentData[1] = x + (i * -1);
                            }
                            break;
                        }else{
                            break;
                        }
                    }
                for (int i = 1; i <= 7; i++){
                    if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }
                    }else if (y + i <= 7 && AnalyzedSideBoard[y + i, x] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + i);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess1");
                for (int i = 1; i <= 7; i++){
                    if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        CanMoveHereData[y + (i * -1), x] = false;
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }
                    }else if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + (i * -1));
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + (i * -1);
                            SentData[1] = x;
                        }
                        break;
                    }else{
                       break;
                    }
                }
                for (int i = 1; i <= 7; i ++){
                    if (x + i <= 7 && AnalyzedSideBoard[y, x + i] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }
                    }else if (x + i <= 7 && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + i;
                        }
                        break;
                    }else{
                        break;
                    }
                }
                //Debug.Log("Sucess3");
                for (int i = 1; i <= 7; i++){
                    if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == 0){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }
                    }else if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + (i * -1), y);
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y;
                            SentData[1] = x + (i * -1);
                        }
                        break;
                    }else{
                        break;
                    }
                }
                Debug.Log("Amazon: Position of Checked Piece: ("+ x +", "+ y +")");
                return SentData;
            }
            //Portal Thief 1 Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 8){
                if (y + 1 <= 7 && AnalyzedSideBoard[y, x] != AnalyzedSideBoard[y + 1, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CanMoveHereData[y + 1, x] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x;
                    }
                }else if (y + 1 <= 7 && AnalyzedSideBoard[y, x] == AnalyzedSideBoard[y + 1, x] && HasType(1, x, y + 1, AnalyzedIntBoard)){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CanMoveHereData[y - 1, x] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x;
                    }
                }
                if (y - 1 >= 0 && AnalyzedSideBoard[y, x] != AnalyzedSideBoard[y - 1, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CanMoveHereData[y - 1, x] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x;
                    }
                }else if (y - 1 >= 0 && AnalyzedSideBoard[y, x] == AnalyzedSideBoard[y - 1, x] && HasType(1, x, y - 1, AnalyzedIntBoard)){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CanMoveHereData[y - 1, x] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x;
                    }
                }
                Debug.Log("Portal Thief 1: Position of Checked Piece: ("+ x +", "+ y +")");
                return SentData;
            }
            //Portal Thief 2 Logic Code
            else if (AnalyzedIntBoard[y, x, m] == 9){
                if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x;
                    }
                }
                if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    CanMoveHereData[y + 1, x + 1] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 1;
                    }
                }
                if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 1;
                    }
                }
                if (y - 1 >= 0 && AnalyzedSideBoard[y - 1, x] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x;
                    }
                }
                if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 1;
                    }
                }
                if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 1;
                    }
                }
                if (x + 1 <= 7 && AnalyzedSideBoard[y, x + 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x + 1;
                    }
                }
                if (x - 1 >= 0 && AnalyzedSideBoard[y, x - 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x - 1;
                    }
                }
                Debug.Log("Portal Thief 2: Position of Checked Piece: ("+ x +", "+ y +")");
                return SentData;
            }
            //Portal Thief 3 Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 10){
                if (y + 1 <= 7 && AnalyzedSideBoard[y + 1, x] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x;
                    }
                }
                if (y + 1 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 1, x + 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 1);
                    CanMoveHereData[y + 1, x + 1] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 1;
                    }
                }
                if (y + 1 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 1, x - 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 1;
                    }
                }
                if (y - 1 >= 0 && AnalyzedSideBoard[y - 1, x] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x;
                    }
                }
                if (y - 1 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 1, x + 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 1;
                    }
                }
                if (y - 1 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 1, x - 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 1;
                    }
                }
                if (x + 1 <= 7 && AnalyzedSideBoard[y, x + 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x + 1;
                    }
                }
                if (x - 1 >= 0 && AnalyzedSideBoard[y, x - 1] != Side){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y;
                        SentData[1] = x - 1;
                    }
                }
                for (int i = -2; i <= 2; i++){
                    int num2 = 0;
                    num2 = x + i;
                    //Debug.Log("X: "+ num2 +" Y: "+ y + 2);
                    if (y - 2 >= 0 && num2 >= 0 && num2 <= 7 && AnalyzedSideBoard[y - 2, x + i] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y - 2);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y - 2);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + i, y - 2);
                        CanMoveHereData[y - 2, x + i] = false;
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y - 2;
                            SentData[1] = x + i;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y - 2;
                            SentData[1] = x + i;
                        }
                    }
                }
                for (int i = -1; i <= 1; i++){
                    int num = 0;
                    num = y + i;
                    if (num <= 7 && num >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y + i, x - 2] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + i);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + i);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + i);
                        CanMoveHereData[y + i, x - 2] = false;
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x - 2;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x - 2;
                        }
                    }
                }
                for (int i = -1; i <= 1; i++){
                    int num = 0;
                    num = y + i;
                    if (num <= 7 && num >= 0 && x + 2 >= 0 && AnalyzedSideBoard[y + i, x + 2] != AnalyzedSideBoard[y, x]){
                        CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                        CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                        CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + i);
                        CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + i);
                        Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + i);
                        CanMoveHereData[y + i, x + 2] = false;
                        if (Evaluation > SentData[0] && Side == 1){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x + 2;
                        }else if (Evaluation < SentData[0] && Side == 2){
                            SentData[0] = Evaluation;
                            SentData[2] = y + i;
                            SentData[1] = x + 2;
                        }
                    }
                }
                Debug.Log("Portal Thief 3: Position of Checked Piece: ("+ x +", "+ y +")");
                return SentData;
            }
            //Smiler Logic Code
            else if (AnalyzedIntBoard[y, x, m] == 11){
                if (y + 1 <= 7 && x + 2 <= 7 && AnalyzedSideBoard[y + 1, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    MoveIntPiece(AnalyzedIntBoard, AnalyzedSideBoard, x, y, x + 2, y + 1);
                    MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[1] = y + 1;
                        SentData[2] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 2;
                    }
                }else if (y + 1 <= 7 && x + 2 <= 7 && AnalyzedSideBoard[y + 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y + 1, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x + 2;
                    }
                }
                if (y + 1 <= 7 && x - 2 >= 0 && AnalyzedSideBoard[y + 1, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 2;
                    }
                }else if (y + 1 <= 7 && x - 2 >= 0 && AnalyzedSideBoard[y + 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y + 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y + 1, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 1;
                        SentData[1] = x - 2;
                    }
                }
                if (y - 1 >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y - 1, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 2;
                    }
                }else if (y - 1 >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y - 1, x - 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x - 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x - 2, y - 1, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x - 2;
                    }
                }
                if (y - 1 >= 0 && x + 2 <= 7 && AnalyzedSideBoard[y - 1, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 2;
                    }
                }else if (y - 1 >= 0 && x + 2 <= 7 && AnalyzedSideBoard[y - 1, x + 2] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 1, x + 2, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 2, y - 1, AnalyzedIntBoard) == false && HasType(6, x + 2, y - 1, AnalyzedIntBoard) == false){
                    CopyIntBoard(CurrentIntBoard, CheckingIntBoard);
                    CopySideBoard(CurrentSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 1);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 1;
                        SentData[1] = x + 2;
                    }
                }
                if (y + 2 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 2, x + 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 1;
                    }
                }else if (y + 2 <= 7 && x + 1 <= 7 && AnalyzedSideBoard[y + 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y + 2, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y + 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 1;
                    }
                }
                if (y + 2 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 2, x - 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 1;
                    }
                }else if (y + 2 <= 7 && x - 1 >= 0 && AnalyzedSideBoard[y + 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y + 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y + 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y + 2, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y + 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 1;
                    }
                }
                if (y - 2 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 2, x + 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 1;
                    }
                }else if (y - 2 >= 0 && x + 1 <= 7 && AnalyzedSideBoard[y - 2, x + 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x + 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x + 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x + 1, y - 2, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 1, y - 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 1;
                    }
                }
                if (y - 2 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 2, x - 1] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 1;
                    }
                }else if (y - 2 >= 0 && x - 1 >= 0 && AnalyzedSideBoard[y - 2, x - 1] == AnalyzedSideBoard[y, x] && HasType(AnalyzedIntBoard[y - 2, x - 1, 0], x, y, AnalyzedIntBoard) == false && HasType(5, x - 1, y - 2, AnalyzedIntBoard) == false && HasType(6, x - 1, y - 2, AnalyzedIntBoard) == false){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 1, y - 2);
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 1;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 1;
                    }
                }
                if (y + 4 <= 7 && x + 2 <= 7 && AnalyzedSideBoard[y + 4, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 4);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 4);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y + 4);
                    CanMoveHereData[y + 4, x + 2] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 4;
                        SentData[1] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 4;
                        SentData[1] = x + 2;
                    }
                }
                if (y + 4 <= 7 && x - 2 >= 0 && AnalyzedSideBoard[y + 4, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 4);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 4);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y + 4);
                    CanMoveHereData[y + 4, x - 2] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 4;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 4;
                        SentData[1] = x - 2;
                    }
                }
                if (y - 4 >= 0 && x + 2 <= 7 && AnalyzedSideBoard[y - 4, x + 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 4);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 4);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 2, y - 4);
                    CanMoveHereData[y - 4, x + 2] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 4;
                        SentData[1] = x + 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 4;
                        SentData[1] = x + 2;
                    }
                }
                if (y - 4 >= 0 && x - 2 >= 0 && AnalyzedSideBoard[y - 4, x - 2] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 4);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 4);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 2, y - 4);
                    CanMoveHereData[y - 4, x - 2] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 4;
                        SentData[1] = x - 2;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 4;
                        SentData[1] = x - 2;
                    }
                }
                if (y + 2 <= 7 && x + 4 <= 7 && AnalyzedSideBoard[y + 2, x + 4] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y + 2);
                    CanMoveHereData[y + 2, x + 4] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 4;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x + 4;
                    }
                }
                if (y - 2 >= 0 && x + 4 <= 7 && AnalyzedSideBoard[y - 2, x + 4] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x + 4, y - 2);
                    CanMoveHereData[y - 2, x + 4] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 4;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x + 4;
                    }
                }
                if (y + 2 <= 7 && x - 4 >= 0 && AnalyzedSideBoard[y + 2, x - 4] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y + 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y + 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y + 2);
                    CanMoveHereData[y + 2, x - 4] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 4;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y + 2;
                        SentData[1] = x - 4;
                    }
                }
                if (y - 2 >= 0 && x - 4 >= 0 && AnalyzedSideBoard[y - 2, x - 4] != AnalyzedSideBoard[y, x]){
                    CopyIntBoard(AnalyzedIntBoard, CheckingIntBoard);
                    CopySideBoard(AnalyzedSideBoard, CheckingSideBoard);
                    CheckingIntBoard = MoveIntPiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y - 2);
                    CheckingSideBoard = MoveSidePiece(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y - 2);
                    Evaluation = EvaluateBoard(CheckingIntBoard, CheckingSideBoard, x, y, x - 4, y - 2);
                    CanMoveHereData[y - 2, x - 4] = false;
                    if (Evaluation > SentData[0] && Side == 1){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 4;
                    }else if (Evaluation < SentData[0] && Side == 2){
                        SentData[0] = Evaluation;
                        SentData[2] = y - 2;
                        SentData[1] = x - 4;
                    }
                }
                Debug.Log("Smiler: Position of Checked Piece: ("+ x +", "+ y +")");
                return SentData;
            }else{
                //Debug.Log("No Correct Piece found Pos: (" + x +", "+ y +")");
            }
            }
        Debug.Log("Didn't find a piece. Piece number given: "+ AnalyzedIntBoard[y, x, m]);
        if (Side == 1 && AnalyzedIntBoard[y, x, m] == 0){
            SentData[0] = -9999999999999999999999f;
        }else if (Side == 2 && AnalyzedIntBoard[y, x, m] == 0){
            SentData[0] = 9999999999999999999999f;
        }
        return SentData;
    }

    public bool BootlegLegalMove(int[,,] AnalyzedIntBoard, int[,] AnalyzedSideBoard, int x, int y, int ToX, int ToY){
        if (ToX == 0 && ToY == 0){
            return false;
        }
        if (AnalyzedIntBoard[y, x, 0] == 6 && AnalyzedSideBoard[ToY, ToX] == AnalyzedSideBoard[y, x]){
            return false;
        }
        if (AnalyzedIntBoard[y, x, 0] == 5 && AnalyzedSideBoard[ToY, ToX] == AnalyzedSideBoard[y, x]){
            return false;
        }
        return true;
    }

    //Need to rewrite. If you want to try to rewrite this good luck. This isin't used anywhere in the code anymore
    public bool IsLegalCaptureMove(int[,,] AnalyzedIntBoard, int[,] AnalyzedSideBoard, int x, int y, int FutX, int FutY){
        for (int m = 0; m <= 15; m++){
            //Debug.Log("Piece Type: "+ AnalyzedIntBoard[y, x, m]);
            //Debug.Log("AtCords: ("+ x +", "+ y +")");
            //Debug.Log("ToCords: ("+ FutX +", "+ FutY +")");
            //Knight Logic Code:
            if (AnalyzedIntBoard[y, x, m] == 2){
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
                        if (y + i <= 7 && x + (i * -1) >= 0 && AnalyzedSideBoard[y + i, x + (i * -1)] == 0){
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
                    if (y + i <= 7 && AnalyzedSideBoard[y + i, x] == 0){
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
                    if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
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
                for (int i = 1; i <= 7; i++){
                    if (i > 7){
                        break;
                    }
                    if (x + i > 7){
                        break;
                    }
                    if (x + i <= 7 && AnalyzedSideBoard[y, x + i] == 0){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                    }else if (x + i <= 7 && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                        break;
                    }else if (x + i <= 7 && AnalyzedSideBoard[y, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y, AnalyzedIntBoard) == false && HasType(6, x + i, y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + i, 0], x, y, AnalyzedIntBoard) == false){
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
                    if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == 0){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                    }else if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                        break;
                    }else if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
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
            //Pawn Logic Code:
            //Moved to optimize making moves with pieces other than pawns when merged
            if (AnalyzedIntBoard[y, x, m] == 1){
                Debug.Log("Side: "+ AnalyzedSideBoard[1, 0]);
                Debug.Log("To Side: "+ AnalyzedSideBoard[3, 0]);
                if (AnalyzedSideBoard[y, x] == 1){ 
                    Debug.Log("A: "+ x);
                    Debug.Log("A: "+ y);
                    if (y == 1){
                        Debug.Log("We're in");
                        if (AnalyzedSideBoard[y + 2, x] == 0){
                            if (y + 2 == FutY && x == FutX){
                                return true;
                            }
                        }
                    }
                    if (y + 1 <= 7 && AnalyzedIntBoard[y + 1, x, 0] == 0){
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
                else if (AnalyzedSideBoard[y, x] == 2){
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
                    if (x + i <= 7 && AnalyzedSideBoard[y, x + i] == 0){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                    }else if (y + i <= 7 && AnalyzedSideBoard[y, x + i] != AnalyzedSideBoard[y, x]){
                        if (y == FutY && x + i == FutX){
                            return true;
                        }
                        break;
                    }else if (y + i <= 7 && AnalyzedSideBoard[y, x + i] == AnalyzedSideBoard[y, x] && HasType(5, x + i, y, AnalyzedIntBoard) == false && HasType(6, x + i, y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + i, 0], x, y, AnalyzedIntBoard) == false){
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
                    if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == 0 && HasType(4, x, y, AnalyzedIntBoard)){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                    }else if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] != AnalyzedSideBoard[y, x]){
                        if (y == FutY && x + (i * -1) == FutX){
                            return true;
                        }
                        break;
                    }else if (x + (i * -1) >= 0 && AnalyzedSideBoard[y, x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y, AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y, x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
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
                    if (y + (i * -1) >= 0 && AnalyzedSideBoard[y + (i * -1), x] == 0){
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

    public float[] RunDepthScan(int[,,] UsedIntBoard, int[,] UsedSideBoard, int CurrentDepth, int GivenSide){
        Debug.Log("___________________New Depth Iteration___________________");
        Debug.Log("Depth Test: "+ UsedIntBoard[1, 3, 0]);
        float[] ReturnInfo = new float[5];
        float[] BackInfo = new float[5];
        float[] GivenInfo = new float[3];
        int SentSide = 0;
        int[,,] SendingIntBoard = new int[8, 8, 16];
        int[,] SendingSideBoard = new int[8, 8];
        int[,,] ProcessingIntBoard = new int[8, 8, 16];
        int[,] ProcessingSideBoard = new int[8, 8];
        bool IsPossible = false;
        SendingIntBoard = UsedIntBoard;
        SendingSideBoard = UsedSideBoard;
        if (CurrentDepth == 0){
            int TempSide = Side;
            Side = GivenSide;
            float Eval = EvaluateBoard(UsedIntBoard, UsedSideBoard, 0, 0, 0, 0);
            Side = TempSide;
            ReturnInfo[0] = Eval;
            ReturnInfo[1] = 0;
            ReturnInfo[2] = 0;
            ReturnInfo[3] = 0;
            ReturnInfo[4] = 0;
            Debug.Log("At end of branch. Returning eval of "+ ReturnInfo[0]);
            Debug.Log("___________________End of Depth Iteration Branch___________________");
        }else{
            if (GivenSide == 1){
                ReturnInfo[0] = -99999999999999999;
                SentSide = 2;
            }else if (GivenSide == 2){
                ReturnInfo[0] = 99999999999999999;
                SentSide = 1;
            }
            for (int y = 0; y <= 7; y++){
                for (int x = 0; x <= 7; x++){
                    for (int m = 0; m <= 7; m++){
                        if (SendingSideBoard[y, x] == GivenSide && SendingIntBoard[y, x, m] != 0){
                            //Commented code here used for debugging
                            /*Debug.Log(SendingIntBoard[7, 0, 0] +" "+ SendingIntBoard[7, 1, 0] +" "+ SendingIntBoard[7, 2, 0] +" "+ SendingIntBoard[7, 3, 0] +" "+ SendingIntBoard[7, 4, 0] +" "+ SendingIntBoard[7, 5, 0] +" "+ SendingIntBoard[7, 6, 0] +" "+ SendingIntBoard[7, 7, 0]);
                            Debug.Log(SendingIntBoard[6, 0, 0] +" "+ SendingIntBoard[6, 1, 0] +" "+ SendingIntBoard[6, 2, 0] +" "+ SendingIntBoard[6, 3, 0] +" "+ SendingIntBoard[6, 4, 0] +" "+ SendingIntBoard[6, 5, 0] +" "+ SendingIntBoard[6, 6, 0] +" "+ SendingIntBoard[6, 7, 0]);
                            Debug.Log(SendingIntBoard[5, 0, 0] +" "+ SendingIntBoard[5, 1, 0] +" "+ SendingIntBoard[5, 2, 0] +" "+ SendingIntBoard[5, 3, 0] +" "+ SendingIntBoard[5, 4, 0] +" "+ SendingIntBoard[5, 5, 0] +" "+ SendingIntBoard[5, 6, 0] +" "+ SendingIntBoard[5, 7, 0]);
                            Debug.Log(SendingIntBoard[4, 0, 0] +" "+ SendingIntBoard[4, 1, 0] +" "+ SendingIntBoard[4, 2, 0] +" "+ SendingIntBoard[4, 3, 0] +" "+ SendingIntBoard[4, 4, 0] +" "+ SendingIntBoard[4, 5, 0] +" "+ SendingIntBoard[4, 6, 0] +" "+ SendingIntBoard[4, 7, 0]);
                            Debug.Log(SendingIntBoard[3, 0, 0] +" "+ SendingIntBoard[3, 1, 0] +" "+ SendingIntBoard[3, 2, 0] +" "+ SendingIntBoard[3, 3, 0] +" "+ SendingIntBoard[3, 4, 0] +" "+ SendingIntBoard[3, 5, 0] +" "+ SendingIntBoard[3, 6, 0] +" "+ SendingIntBoard[3, 7, 0]);
                            Debug.Log(SendingIntBoard[2, 0, 0] +" "+ SendingIntBoard[2, 1, 0] +" "+ SendingIntBoard[2, 2, 0] +" "+ SendingIntBoard[2, 3, 0] +" "+ SendingIntBoard[2, 4, 0] +" "+ SendingIntBoard[2, 5, 0] +" "+ SendingIntBoard[2, 6, 0] +" "+ SendingIntBoard[2, 7, 0]);
                            Debug.Log(SendingIntBoard[1, 0, 0] +" "+ SendingIntBoard[1, 1, 0] +" "+ SendingIntBoard[1, 2, 0] +" "+ SendingIntBoard[1, 3, 0] +" "+ SendingIntBoard[1, 4, 0] +" "+ SendingIntBoard[1, 5, 0] +" "+ SendingIntBoard[1, 6, 0] +" "+ SendingIntBoard[1, 7, 0]);
                            Debug.Log(SendingIntBoard[0, 0, 0] +" "+ SendingIntBoard[0, 1, 0] +" "+ SendingIntBoard[0, 2, 0] +" "+ SendingIntBoard[0, 3, 0] +" "+ SendingIntBoard[0, 4, 0] +" "+ SendingIntBoard[0, 5, 0] +" "+ SendingIntBoard[0, 6, 0] +" "+ SendingIntBoard[0, 7, 0]);
                            */
                            if (UsedSideBoard[y, x] == GivenSide){
                                int TempSide = Side;
                                //If you try to make the ProcessingIntBoard = SendingIntBoard it breaks the code
                                CopyIntBoard(SendingIntBoard, ProcessingIntBoard);
                                CopySideBoard(SendingSideBoard, ProcessingSideBoard);
                                Side = GivenSide;
                                GivenInfo = NextLegalMove(ProcessingIntBoard, ProcessingSideBoard, x, y, m, GivenSide);
                                ProcessingIntBoard = MoveIntPiece(ProcessingIntBoard, ProcessingSideBoard, x, y, (int) GivenInfo[1], (int) GivenInfo[2]);
                                ProcessingSideBoard = MoveSidePiece(ProcessingIntBoard, ProcessingSideBoard, x, y, (int) GivenInfo[1], (int) GivenInfo[2]);
                                if (BootlegLegalMove(ProcessingIntBoard, ProcessingSideBoard, x, y, (int) GivenInfo[1], (int) GivenInfo[2]) == true){
                                    BackInfo = RunDepthScan(ProcessingIntBoard, ProcessingSideBoard, CurrentDepth - 1, SentSide);
                                    IsPossible = true;
                                }else{
                                    IsPossible = false;
                                }
                                Side = TempSide;
                                Debug.Log("Now Side: "+ Side);
                                if (IsPossible == true && BackInfo[0] >= ReturnInfo[0] && GivenSide == 1){
                                    ReturnInfo[0] = BackInfo[0];
                                    ReturnInfo[1] = x;
                                    ReturnInfo[2] = y;
                                    ReturnInfo[3] = GivenInfo[1];
                                    ReturnInfo[4] = GivenInfo[2];
                                }else if (IsPossible == true && BackInfo[0] <= ReturnInfo[0] && GivenSide == 2){
                                    ReturnInfo[0] = BackInfo[0];
                                    ReturnInfo[1] = x;
                                    ReturnInfo[2] = y;
                                    ReturnInfo[3] = GivenInfo[1];
                                    ReturnInfo[4] = GivenInfo[2];
                                    Debug.Log("Position: ("+ x +", "+ y +") to ("+ ReturnInfo[3] +", "+ ReturnInfo[4] +") With an evaluation of "+ ReturnInfo[0]);
                                }
                            }
                        }
                    }
                }
            }
            Debug.Log("Info sending back: Depth: "+ CurrentDepth +" Eval: "+ ReturnInfo[0] +" Starting pos: ("+ ReturnInfo[1] +", "+ ReturnInfo[2] +") Ending pos: ("+ ReturnInfo[3] +", "+ ReturnInfo[4] +")");
            Debug.Log("___________________End of Depth Iteration___________________");
        }
        return ReturnInfo;
    }

    public void VeryDrunk(int[,,] GivenIntBoard){
        bool loop = true;
        int x = 0;
        int y = 0;
        int tox = 0;
        int toy = 0;
        while (loop == true){
            x = Random.Range(0, 7);
            y = Random.Range(0, 7);
            tox = Random.Range(0, 7);
            toy = Random.Range(0, 7);
            if (GivenIntBoard[y, x, 0] != 0){
                MoveOnPlayerBoard(x, y, tox, toy);
                loop = false;
            }
        }
    }

    public void MakeMove(){
        float[] ReturnData = new float[5];
        CreateBoardSnapshot();
        //CopyIntBoard(CurrentIntBoard, CheckingIntBoard);
        //CopySideBoard(CurrentSideBoard, CheckingSideBoard);
        //PlayRandomMove(CurrentIntBoard, CurrentSideBoard);
        if (VeryDrunkMode == true){
            VeryDrunk(CurrentIntBoard);
        }else{
            //Debug.Log(CurrentIntBoard[1, 3, 0]);
            ReturnData = RunDepthScan(CurrentIntBoard, CurrentSideBoard, MaxDepth, Side);
            MoveOnPlayerBoard((int) ReturnData[1],(int) ReturnData[2],(int) ReturnData[3],(int) ReturnData[4]);
        }
    }

    void OnMouseDown(){
        MakeMove();
    }

    public void PlayRandomMove(int[,,] GivenIntBoard, int[,] GivenSideBoard){
        bool Check = true;
        int x = 0;
        int y = 0;
        float[] GivenData = new float[3];
        if (Check == true){
            x = Random.Range(0, 7);
            y = Random.Range(0, 7);
            if (GivenSideBoard[y, x] == Side){
                for (int m = 0; m <= 7; m++){
                    GivenData = NextLegalMove(GivenIntBoard, GivenSideBoard, x, y, m, Side);
                }
                Check = false;
            }
        }
        MoveOnPlayerBoard(x, y, (int) GivenData[1], (int) GivenData[2]);
    }

    public void RandomizeEvalValues(){
        PawnMaterialBoard = RandomizeMaterialBoard(PawnMaterialBoard);
        KnightMaterialBoard = RandomizeMaterialBoard(KnightMaterialBoard);
        BishopMaterialBoard = RandomizeMaterialBoard(BishopMaterialBoard);
        RookMaterialBoard = RandomizeMaterialBoard(RookMaterialBoard);
        QueenMaterialBoard = RandomizeMaterialBoard(QueenMaterialBoard);
        KingMaterialBoard = RandomizeMaterialBoard(KingMaterialBoard);
        CenterImportanceMult = Random.Range(0.000000001f, 99999999f);
        MaterialMultiplier = Random.Range(0.000000001f, 99999999f);
        SpecialPeiceMoveReward = Random.Range(0.000000001f, 99999999f);
    }

    // Start is called before the first frame update
    void Start(){
        RandomizeEvalValues();
    }

    // Update is called once per frame
    void Update(){
        
    }
}
