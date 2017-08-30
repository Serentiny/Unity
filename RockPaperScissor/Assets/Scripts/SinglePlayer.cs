using UnityEngine;
using UnityEngine.UI;

public class SinglePlayer : MonoBehaviour
{
    public Canvas canvasSingle, canvasMainMenu;
    public Text avatarPlayerName, counter;
    public Image leftNothing, leftRock, leftPaper, leftScissors, rightNothing, rightRock, rightPaper, rightScissors;

    static int w_count, l_count;
    static string playerName;

    int lastMoves;

    //=====================================================================================================================

    void Start()
    {
        ClearAll(true);
    }

    void Update()
    {
        avatarPlayerName.text = playerName;
        counter.text = w_count + " : " + l_count;
    }

    //=====================================================================================================================

    public static void StartNewGame()
    {
        w_count = Account.GetSpWin();
        l_count = Account.GetSpLoose();
        playerName = Account.GetName();
    }

    //=====================================================================================================================

    public void ChooseRock()
    {
        Account.AddNewMove((int)Moves.Rock);
        ClearAll(false);
        leftRock.gameObject.SetActive(true);
        AITurn();
    }
    public void ChoosePaper()
    {
        Account.AddNewMove((int)Moves.Paper);
        ClearAll(false);
        leftPaper.gameObject.SetActive(true);
        AITurn();
    }
    public void ChooseScissors()
    {
        Account.AddNewMove((int)Moves.Scissors);
        ClearAll(false);
        leftScissors.gameObject.SetActive(true);
        AITurn();
    }

    void AITurn()
    {
        lastMoves = Account.GetLastMoves();
        int playerMove = lastMoves % 10;

        int r = 0, p = 0, s = 0;
        double perR = 0, perP = 0, perS = 0;
        while (lastMoves > 0)
        {
            int last = lastMoves % 10;
            lastMoves /= 10;
            switch (last)
            {
                case (int)Moves.Rock:
                    r++;
                    break;
                case (int)Moves.Paper:
                    p++;
                    break;
                case (int)Moves.Scissors:
                    s++;
                    break;
                default:
                    break;
            }
        }
        double rand = Random.Range(0, 100);
        int size = r + p + s;
        perP = 100 * r / size;
        perS = 100 * p / size;
        perR = 100 * s / size;
        
        int aiMove;
        if (rand > perR)
        {
            if (rand > perR + perP) //Scissors
            {
                rightScissors.gameObject.SetActive(true);
                aiMove = (int)Moves.Scissors;
            }
            else                    //Paper
            {
                rightPaper.gameObject.SetActive(true);
                aiMove = (int)Moves.Paper;
            }
        }
        else                        //Rock
        {
            rightRock.gameObject.SetActive(true);
            aiMove = (int)Moves.Rock;
        }

        if (rand > perR + perP + perS)
            Debug.Log("OMG! =)");

        //Check
        if (playerMove == (int)Moves.Scissors && aiMove == (int)Moves.Paper    ||
            playerMove == (int)Moves.Rock     && aiMove == (int)Moves.Scissors ||
            playerMove == (int)Moves.Paper    && aiMove == (int)Moves.Rock)
        {
            Account.AddSpWin();
            w_count++;
        }

        if (playerMove == (int)Moves.Rock     && aiMove == (int)Moves.Paper    ||
            playerMove == (int)Moves.Paper    && aiMove == (int)Moves.Scissors ||
            playerMove == (int)Moves.Scissors && aiMove == (int)Moves.Rock)
        {
            Account.AddSpLoose();
            l_count++;
        }

        Account.QuickSave();
    }

    public void PressBack()
    {
        canvasMainMenu.gameObject.SetActive(true);
        canvasSingle.gameObject.SetActive(false);
        
        ClearAll(true);
    }
    
    /// <summary>
    /// nothing - оставлять ли знак вопроса
    /// </summary>
    /// <param name="nothing"></param>
    void ClearAll(bool nothing)
    {
        leftRock.gameObject.SetActive(false);
        leftPaper.gameObject.SetActive(false);
        leftScissors.gameObject.SetActive(false);
        leftNothing.gameObject.SetActive(nothing);

        rightRock.gameObject.SetActive(false);
        rightPaper.gameObject.SetActive(false);
        rightScissors.gameObject.SetActive(false);
        rightNothing.gameObject.SetActive(nothing);
    }
}
