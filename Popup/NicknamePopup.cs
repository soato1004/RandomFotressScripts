using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress
{
    public class NicknamePopup : PopupBase
    {
        [SerializeField] protected Button oneButton;
        [SerializeField] private TextMeshProUGUI nickname;
        [SerializeField] private TextMeshProUGUI explan;

        private TextMeshProUGUI buttonText;
        
        private readonly Regex ValidNicknameRegex = new Regex(@"^[a-zA-Z0-9가-힣_-]+$");
        private readonly Regex EmojiRegex = new Regex(@"[\u1F600-\u1F64F\u1F300-\u1F5FF\u1F680-\u1F6FF\u1F1E0-\u1F1FF]");
        private readonly Regex KoreanConsonantVowelRegex = new Regex(@"^[\u3131-\u314E\u314F-\u3163]+$");
        
        public override void ShowPopup(params object[] values)
        {
            // 닉네임 확인버튼 클릭
            oneButton?.onClick.RemoveAllListeners();
            oneButton?.onClick.AddListener(OnOkButtonClick);

            buttonText = oneButton.GetComponentInChildren<TextMeshProUGUI>();

            base.ShowPopup();
        }

        // 닉네임 체크하기
        private void OnOkButtonClick()
        {
            if (nickname.text == "")
                return;
            
            string cleanedNickname = CleanNickname(nickname.text);
            string errorMessage = CheckNicknameValidity(cleanedNickname);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                explan.text = LocalizationManager.I.GetLocalizedString(errorMessage);
                StartCoroutine(ButtonLockCor());
            }
            else
            {
                CheckNickNameCor(cleanedNickname);
                // StartCoroutine(CheckNickNameCor(cleanedNickname));
            }
        }

        private string CleanNickname(string input)
        {
            // 공백 제거 및 보이지 않는 문자 제거
            return Regex.Replace(input.Trim(), @"[\s\u200B-\u200D\uFEFF]+", "");
        }

        private string CheckNicknameValidity(string nickName)
        {
            if (nickName.Length < 3 || nickName.Length > 12)
                return "error_nickname_length";

            if (char.IsDigit(nickName[0]))
                return "error_nickname_start_with_number";

            if (nickName.All(char.IsDigit))
                return "error_nickname_only_numbers";

            if (KoreanConsonantVowelRegex.IsMatch(nickName))
                return "error_nickname_invalid";
            
            if (EmojiRegex.IsMatch(nickName))
                return "error_nickname_invalid";
            
            if (!ValidNicknameRegex.IsMatch(nickName))
                return "error_nickname_invalid";
            
            return "";
        }

        private IEnumerator ButtonLockCor(float time = 0.3f)
        {
            oneButton.interactable = false;
            yield return new WaitForSeconds(time);
            oneButton.interactable = true;
        }

        private async void CheckNickNameCor(string nicknameText)
        {
            oneButton.interactable = false;
            buttonText.DOFade(0.3f, 0.1f);
            
            bool isAccountCreated = await Account.I.CreateNewAccount(nicknameText);
            if (isAccountCreated)
            {
                RemovePopup();
                return;
            }

            // 상태 반영
            oneButton.interactable = true;
            buttonText.DOFade(1f, 0.1f);
            explan.SetText(Account.I.NicknameExplanation);
            nickname.text = "";
        }
    }
}