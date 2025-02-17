﻿using LitJson;
using System.Text;
using UnityEngine;
using Utils;
using QFramework;

public class RegisterCommand : AbstractCommand {
    private string mUserName;
    private string mPassword;

    public RegisterCommand(string userName, string password) {
        mUserName = userName;
        mPassword = password;
    }

    protected override void OnExecute() {
        StringBuilder sb = new StringBuilder();
        JsonWriter w = new JsonWriter(sb);
        w.WriteObjectStart();
        w.WritePropertyName("UserName");
        w.Write(mUserName);
        w.WritePropertyName("Password");
        w.Write(mPassword);
        w.WriteObjectEnd();
        Debug.Log("++++++ " + sb.ToString());
        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        CoroutineController.manager.StartCoroutine(this.GetSystem<INetworkSystem>().POSTHTTP(url: this.GetSystem<INetworkSystem>().HttpBaseUrl + RequestUrl.registerUrl,
            data: bytes, succData: (data) => {
                this.GetModel<IGameControllerModel>().Token.Value = (string)data["token"];
                this.GetSystem<IDataParseSystem>().ParseUserInfo(data);

                this.GetModel<IUserModel>().Password.Value = mPassword;
            }, code: (code) => {
                if (code == 200) {
                    this.GetSystem<IVibrationSystem>().Haptic();
                    this.GetModel<IGameControllerModel>().WarningAlert.ShowWithText(text: this.GetSystem<II18NSystem>().GetText("Registration Successful"), callback: () => {
                        Util.LoadingScene(SceneID.Index);
                    });
                } else if (code == 423) {
                    this.GetModel<IGameControllerModel>().WarningAlert.ShowWithText(this.GetSystem<II18NSystem>().GetText("User registered"));
                } else if (code == 425) {
                    this.GetModel<IGameControllerModel>().WarningAlert.ShowWithText(this.GetSystem<II18NSystem>().GetText("Incorrect information format"));
                } else {
                    this.GetModel<IGameControllerModel>().WarningAlert.ShowWithText(this.GetSystem<II18NSystem>().GetText("Unknown Error"));
                }
            }));
    }
}