using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBot5 : MonoBehaviour
{
    [SerializeField] int CenterImportanceMult;
    [SerializeField] int MoveCountMult;
    [SerializeField] int PieceAmountMult;
    [SerializeField] int SpecialPeiceMoveReward;
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

    //Returns the amount of material that the piece on the given coordinates is worth
    public int GetMaterial(int x, int y, int[,,] AnalyzedIntBoard, int CheckedSide){
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

    //Evaluates the given position and returns the evaluation assigned to the position
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
        int EKingPosX = 0;
        int EKingPosY = 0;
        float Total = 0.0f;
        Total = 0f;
        //Debug.Log("King Pos: "+ AnalyzedIntBoard[7, 4, 0]);
        //Debug.Log("King Pos: "+ CurrentIntBoard[7, 4, 0]);
        int MoveCount = 0;
        int PieceAmount = 0;
        for (int y = 7; y >= 0; y--){
            for (int x = 7; x >= 0; x--){
                if (AnalyzedIntBoard[y, x, 0] == 6 && AnalyzedSideBoard[y, x] == Side && KingLost == true){
                    //Debug.Log("King Found on ("+ x +", "+ y +")");
                    KingLost = false;
                    KingPosX = x;
                    KingPosY = y;
                }
                if (AnalyzedIntBoard[y, x, 0] == 5 && AnalyzedSideBoard[y, x] == Side && QueenLost == true){
                    //Debug.Log("Queen Found on ("+ x +", "+ y +")");
                    QueenLost = false;
                    QueenPosX = x;
                    QueenPosY = y;
                }
                if (AnalyzedIntBoard[y, x, 0] == 6 && AnalyzedSideBoard[y, x] != Side && EKingLost == true){
                    //Debug.Log("Enemy King Found on ("+ x +", "+ y +")");
                    EKingLost = false;
                    EKingPosX = x;
                    EKingPosY = y;
                }
                if (AnalyzedIntBoard[y, x, 0] == 5 && AnalyzedSideBoard[y, x] != Side && EQueenLost == true){
                    //Debug.Log("Enemy Queen Found on ("+ x +", "+ y +")");
                    EQueenLost = false;
                }
                if (AnalyzedSideBoard[y, x] == Side){
                    for (int m = 0; m <= 15; m++){
                        if (AnalyzedIntBoard[y, x, m] == 0){
                            break;
                        }else if (AnalyzedIntBoard[y, x, m] != 0){
                            PieceAmount++;
                        }
                    }
                    for (int d = 0; d <= 7; d++){
                        for (int n = 0; n <= 7; n++){
                            if (IsLegalCaptureMove(AnalyzedIntBoard, AnalyzedSideBoard, y, x, n, d) == true){
                                MoveCount++;
                            }
                        }
                    }
                }
            }
        }
        Total += MoveCount * MoveCountMult * SideChanger;
        Total += PieceAmount * PieceAmountMult * SideChanger;
        if (QueenLost == true){
            //Debug.Log("Queen Lost");
            Total += -999999999f * SideChanger;
        }
        if (KingLost == true){
            //Debug.Log("King Lost");
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
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                if (AnalyzedSideBoard[y, x] != 0 && AnalyzedSideBoard[y, x] != Side && IsLegalCaptureMove(AnalyzedIntBoard, AnalyzedSideBoard, x, y, KingPosX, KingPosY) == true){
                    Total += -9999999999999 * SideChanger;
                }
                if (AnalyzedSideBoard[y, x] != 0 && AnalyzedSideBoard[y, x] != Side && IsLegalCaptureMove(AnalyzedIntBoard, AnalyzedSideBoard, x, y, QueenPosX, QueenPosY) == true){
                    Total += -9999999999999 * SideChanger;
                }
            }
        }
        Total = Total / 64.0f;
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
        //Debug.Log("The Square Control Amount is: "+ SquareControlAmount);
        int MaterialAmount = BoardManagerObj.GetComponent<BoardManager>().GetAverageMaterial();
        Total += (float) MaterialAmount * 9999999999 * SideChanger;
        //Debug.Log("The Material Score is: "+ MaterialAmount);
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
        //Debug.Log("The king defense total is: "+ KingDefenseTotal);
        Total += KingDefenseTotal;
        Debug.Log("The Total evaluation is: "+ Total);
        float[] ReturnData = new float[5];
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
        Debug.Log("An: "+ ChangedSideBoard[dy, dx]);
        Debug.Log("Bn: "+ ChangedSideBoard[y, x]);
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
    public int[,,] MoveIntPiece(int[,,] ChangedIntBoard, int[,] ChangedSideBoard, int dx, int dy, int x, int y){
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
            //Debug.Log("We're in");
            if (dx + 2 <= 7 && (x + 2 == dx && y + 1 == dy) || (x + 2 == dx && y - 1 == dy)){
                //Debug.Log("X: "+ x +" Y: "+ y);
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y, x + 1, i] = 0;
                    ChangedIntBoard[y, x + 2, i] = 0;
                }
            }
            if ( x - 2 >= 0 && ((y + 1 <= 7 && x - 2 == dx && y + 1 == dy) || (y - 1 >= 0 && x - 2 == dx && y - 1 == dy))){
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y, x - 1, i] = 0;
                    ChangedIntBoard[y, x - 2, i] = 0;
                }
            }
            if (y + 2 <= 7 && (x + 1 == dx && y + 2 == dy) || (x - 1 == dx && y + 2 == dy)){
                for (int i = 0; i <= 15; i++){
                    ChangedIntBoard[y + 1, x, i] = 0;
                    ChangedIntBoard[y + 2, x, i] = 0;
                }
            }
            if ((x + 1 == dx && y - 2 == dy) || (x - 1 == dx && y - 2 == dy)){
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
                    Debug.Log("1 Created at ("+ t +", "+ i +")");
                }else if (Side1 == false && CurrentObjBoard[i, t] != null){
                    CurrentSideBoard[i, t] = 2;
                    Debug.Log("2 Created");
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
            }
        }
        Debug.Log("Board Created");
        /*for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){
                Debug.Log(CurrentSideBoard[y, x]);
            }
        }*/
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
        Debug.Log("Running a new piece search");
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
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
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
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] == AnalyzedSideBoard[y, x] && HasType(5, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(6, x + (i * -1), y + (i * -1), AnalyzedIntBoard) == false && HasType(AnalyzedIntBoard[y + (i * -1), x + (i * -1), 0], x, y, AnalyzedIntBoard) == false){
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
                        Debug.Log(y);
                        Debug.Log(x);
                        if (AnalyzedSideBoard[y + 2, x] == 0){
                            Debug.Log("In Pawn Code");
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
                    if (y >= 0 && AnalyzedIntBoard[y - 1, x, 0] == 0){
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
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
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
                Debug.Log("Queen: Position of checked piece: ("+ x +", "+ y +")");
            }
            //King Logic Code:
            else if (AnalyzedIntBoard[y, x, m] == 6){
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
                        SentData[1] = y + 1;
                        SentData[2] = x + 2;
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
                        }else if (y + (i * -1) >= 0 && x + (i * -1) <= 7 && AnalyzedSideBoard[y + (i * -1), x + (i * -1)] != AnalyzedSideBoard[y, x]){
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

    //Need to rewrite
    public bool IsLegalCaptureMove(int[,,] AnalyzedIntBoard, int[,] AnalyzedSideBoard, int x, int y, int FutX, int FutY){
        for (int m = 0; m <= 15; m++){
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
        float[] ReturnInfo = new float[5];
        float[] BackInfo = new float[5];
        float[] GivenInfo = new float[3];
        int SentSide = 0;
        int[,,] SendingIntBoard = new int[8, 8, 16];
        int[,] SendingSideBoard = new int[8, 8];
        bool IsPossible = false;
        if (CurrentDepth == 0){
            int TempSide = GivenSide;
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
                        if (UsedSideBoard[y, x] == GivenSide && UsedIntBoard[y, x, m] != 0){
                            if (UsedSideBoard[y, x] == GivenSide){
                                int TempSide = GivenSide;
                                Side = GivenSide;
                                GivenInfo = NextLegalMove(UsedIntBoard, UsedSideBoard, x, y, m, GivenSide);
                                Debug.Log("Given info: Start Pos: ("+ x +", "+ y +") To Pos: ("+ GivenInfo[1] +", "+ GivenInfo[2] +")");
                                SendingIntBoard = UsedIntBoard;
                                SendingSideBoard = UsedSideBoard;
                                Debug.Log("Sid1: "+ UsedSideBoard[3, 0]);
                                SendingIntBoard = MoveIntPiece(UsedIntBoard, UsedSideBoard, x, y, (int) GivenInfo[1], (int) GivenInfo[2]);
                                SendingSideBoard = MoveSidePiece(UsedIntBoard, UsedSideBoard, x, y, (int) GivenInfo[1], (int) GivenInfo[2]);
                                Debug.Log("Sid2: "+ UsedSideBoard[3, 0]);
                                if (IsLegalCaptureMove(SendingIntBoard, SendingSideBoard, (int) GivenInfo[1], (int) GivenInfo[2], x, y) == true){
                                    BackInfo = RunDepthScan(SendingIntBoard, SendingSideBoard, CurrentDepth - 1, SentSide);
                                    IsPossible = true;
                                    Debug.Log("Move is Possible from ("+ x +", "+ y +") to ("+ GivenInfo[1] +", "+ GivenInfo[2] +")");
                                    Debug.Log(IsPossible);
                                }else{
                                    IsPossible = false;
                                    Debug.Log("Move is not Possible from ("+ x +", "+ y +") to ("+ GivenInfo[1] +", "+ GivenInfo[2] +")");
                                    Debug.Log(IsPossible);
                                }
                                Side = TempSide;
                                /*if (UsedBoard == true){
                                    CopyIntBoard(UsedIntBoard, CheckingIntBoard2);
                                    CopySideBoard(UsedSideBoard, CheckingSideBoard2);
                                    MovePiece(CheckingIntBoard2, CheckingSideBoard2, (int) GivenInfo[1], (int) GivenInfo[2], x, y);
                                    BackInfo = RunDepthScan(CheckingIntBoard2, CheckingSideBoard2, CurrentDepth - 1, SentSide, false);
                                    if (IsLegalCaptureMove(CheckingIntBoard2, CheckingSideBoard2, x, y, (int) GivenInfo[1], (int) GivenInfo[2]) == true){
                                        IsPossible = true;
                                        Debug.Log("Move is Possible from ("+ x +", "+ y +") to ("+ GivenInfo[1] +", "+ GivenInfo[2] +")");
                                        Debug.Log(IsPossible);
                                    }else{
                                        IsPossible = false;
                                        Debug.Log("Move is not Possible from ("+ x +", "+ y +") to ("+ GivenInfo[1] +", "+ GivenInfo[2] +")");
                                        Debug.Log(IsPossible);
                                    }
                                }else if (UsedBoard == false){
                                    CopyIntBoard(UsedIntBoard, CheckingIntBoard);
                                    CopySideBoard(UsedSideBoard, CheckingSideBoard);
                                    MovePiece(CheckingIntBoard, CheckingSideBoard, x, y, (int) GivenInfo[1], (int) GivenInfo[2]);
                                    BackInfo = RunDepthScan(CheckingIntBoard, CheckingSideBoard, CurrentDepth - 1, SentSide, true);
                                    if (IsLegalCaptureMove(CheckingIntBoard, CheckingSideBoard, x, y, (int) GivenInfo[1], (int) GivenInfo[2]) == true){
                                        IsPossible = true;
                                        Debug.Log("Move is Possible from ("+ x +", "+ y +") to ("+ GivenInfo[1] +", "+ GivenInfo[2] +")");
                                        Debug.Log(IsPossible);
                                    }else{
                                        IsPossible = false;
                                        Debug.Log("Move is not Possible from ("+ x +", "+ y +") to ("+ GivenInfo[1] +", "+ GivenInfo[2] +")");
                                        Debug.Log(IsPossible);
                                    }
                                }*/
                                if (IsPossible == true && BackInfo[0] >= ReturnInfo[0] && GivenSide == 1){
                                    ReturnInfo[0] = BackInfo[0];
                                    ReturnInfo[1] = x;
                                    ReturnInfo[2] = y;
                                    ReturnInfo[3] = GivenInfo[1];
                                    ReturnInfo[4] = GivenInfo[2];
                                    Debug.Log(ReturnInfo);
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

    public void FindBestMove(){
        for (int y = 0; y <= 7; y++){
            for (int x = 0; x <= 7; x++){

            }
        }
    }

    void OnMouseDown(){
        float[] ReturnData = new float[5];
        CreateBoardSnapshot();
        //CopyIntBoard(CurrentIntBoard, CheckingIntBoard);
        //CopySideBoard(CurrentSideBoard, CheckingSideBoard);
        //PlayRandomMove(CurrentIntBoard, CurrentSideBoard);
        Debug.Log(CurrentSideBoard[1, 0]);
        ReturnData = RunDepthScan(CurrentIntBoard, CurrentSideBoard, MaxDepth, Side);
        MoveOnPlayerBoard((int) ReturnData[1],(int) ReturnData[2],(int) ReturnData[3],(int) ReturnData[4]);
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
    // Start is called before the first frame update
    void Start(){
        /*CreateBoardSnapshot();
        float Eval = EvaluateBoard(CurrentIntBoard, CurrentSideBoard, 0, 0, 0, 0);
        Debug.Log("The eval is: "+ Eval);*/
    }

    // Update is called once per frame
    void Update(){
        
    }
}
