using System;
using MCGalaxy;
using MCGalaxy.Events;
using MCGalaxy.Events.PlayerEvents;

namespace PluginLockedModel {    
    public sealed class Core : Plugin_Simple {
        public override string creator { get { return "Not UnknownShadow200"; } }
        public override string name { get { return "LockedModel"; } }
        public override string MCGalaxy_Version { get { return "1.9.0.1"; } }
        
        public override void Load(bool startup) {
            OnSendingMotdEvent.Register(OnSendMOTD, Priority.Low);
            OnPlayerCommandEvent.Register(OnPlayerCommand, Priority.Low);
        }
        
        public override void Unload(bool shutdown) {
            OnSendingMotdEvent.Unregister(OnSendMOTD);
            OnPlayerCommandEvent.Unregister(OnPlayerCommand);
        }
        
        void OnSendMOTD(Player p, byte[] motdPacket) {
            string[] models = GetLockedModels(p.level.GetMotd(p));
            const string key = "US200.LockedModel.Model";
            
            if (models == null) {
                // Model user had before joining a level with locked model
                string originalModel = p.Extras.GetString(key);
                if (originalModel == null) return;

                // Restore the model back
                p.Extras.Remove(key);
                p.ScaleX = (float)p.Extras.Get(key + "_X");
                p.ScaleY = (float)p.Extras.Get(key + "_Y");
                p.ScaleZ = (float)p.Extras.Get(key + "_Z");
                Entities.UpdateModel(p, originalModel);
            } else if (!ContainsCaseless(models, p.Model)) {
                // Switch user to the level's locked model
                string currentModel = p.Model;
                float curX = p.ScaleX, curY = p.ScaleY, curZ = p.ScaleZ;               
                p.ScaleX = 0; p.ScaleY = 0; p.ScaleZ = 0;
                Entities.UpdateModel(p, models[0]);
                
                // Don't overwrite model user had before joining a level with locked model
                string originalModel = p.Extras.GetString(key);
                if (originalModel != null) return;
                
                p.Extras.PutString(key, currentModel);
                p.Extras[key + "_X"] = curX;
                p.Extras[key + "_Y"] = curY;
                p.Extras[key + "_Z"] = curZ;
            }
        }
        
        void OnPlayerCommand(Player p, string cmd, string args) {
            if (!(cmd == "model" || cmd == "mymodel")) return;
            if (args.IndexOf(' ') >= 0) return; // using model on another player or bot
            
            string[] models = GetLockedModels(p.level.GetMotd(p));
            if (models == null) return;
            
            if (!ContainsCaseless(models, args)) {
                Player.Message(p, "&cYou may only change your own model to: %S{0}", models.Join());
                p.cancelcommand = true;
            }
        }
        
        
        static string[] GetLockedModels(string motd) {
            // Does the motd have 'model=' in it?
            int index = motd.IndexOf("model=");
            if (index == -1) return null;
            motd = motd.Substring(index + "model=".Length);
            
            // Get the single word after 'model='
            if (motd.IndexOf(' ') >= 0)
                motd = motd.Substring(0, motd.IndexOf(' '));
            
            // Is there an actual word after 'model='?
            if (motd.Length == 0) return null;
            return motd.Split(splitChars);
        }
                
        // reuse single instance to minimise mem allocations
        static char[] splitChars = new char[] { ',' };
        
        static bool ContainsCaseless(string[] a, string b) {
            for (int i = 0; i < a.Length; i++) {
                if (a[i].CaselessEq(b)) return true;
            }
            return false;
        }
    }
}