namespace Scellecs.Morpeh.Utils.Editor.Discover.Actions {
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;
    using UnityEngine;

    [CreateAssetMenu(menuName = "ECS/Utils/Discover Actions/" + "Package")]
    public sealed class DiscoverPackageDescription : DiscoverAction {
        public string packageName;
        public string packageURL;

        private State state = State.None;
        private ListRequest listRequest;
        private AddRequest addRequest;
        //private RemoveRequest removeRequest;

        public override string ActionName {
            get {
                switch (this.state) {
                    case State.None:
                        return "Load...";
                    case State.Exist:
                        return "Force Update";
                    case State.NotExist:
                        return "Install";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void DoAction() {
            switch (this.state) {
                case State.None:
                    break;
                case State.Exist:
                    // this.removeRequest       =  Client.Remove(this.packageName);
                    // EditorApplication.update += this.RemoveProgress;
                    // break;
                case State.NotExist:
                    this.addRequest          =  Client.Add(this.packageURL);
                    EditorApplication.update += this.AddProgress;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum State {
            None = 0,
            Exist = 10,
            NotExist = 20
        }

        private void OnEnable() {
            this.listRequest         = Client.List(true, true);
            EditorApplication.update += this.ListProgress;
        }
        
        // private void RemoveProgress() {
        //     var req = this.removeRequest;
        //     if (req.IsCompleted)
        //     {
        //         if (req.Status == StatusCode.Success) {
        //             this.state = State.NotExist;
        //         }
        //         else if (req.Status >= StatusCode.Failure) {
        //             Debug.Log(req.Error.message);
        //         }
        //
        //         EditorApplication.update -= this.RemoveProgress;
        //     }
        // }

        private void AddProgress() {
            var req = this.addRequest;
            if (req.IsCompleted)
            {
                if (req.Status == StatusCode.Success) {
                    this.state = State.Exist;
                }
                else if (req.Status >= StatusCode.Failure) {
                    Debug.Log(req.Error.message);
                }

                EditorApplication.update -= this.AddProgress;
            }
        }
        
        private void ListProgress()
        {
            var req = this.listRequest;
            if (req.IsCompleted)
            {
                if (req.Status == StatusCode.Success) {
                    if (req.Result.Any(p => p.name == this.packageName)) {
                        this.state = State.Exist;
                    }
                    else {
                        this.state = State.NotExist;
                    }
                }
                else if (req.Status >= StatusCode.Failure) {
                    Debug.Log(req.Error.message);
                }

                EditorApplication.update -= this.ListProgress;
            }
        }
    }

}
