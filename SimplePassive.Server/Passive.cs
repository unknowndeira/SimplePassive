﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;

namespace SimplePassive.Server
{
    /// <summary>
    /// Script that handle the Passive Mode between the clients.
    /// </summary>
    public class Passive : BaseScript
    {
        #region Fields

        /// <summary>
        /// The activation of passive mode for specific players.
        /// </summary>
        public readonly Dictionary<string, bool> activations = new Dictionary<string, bool>();

        #endregion

        #region Properties

        /// <summary>
        /// If the local player has passive enabled or disabled.
        /// </summary>
        public bool DefaultActivation => Convert.ToBoolean(API.GetConvarInt("simplepassive_default", 0));

        #endregion

        #region Tools

        /// <summary>
        /// Gets the activation of passive mode for a specific player.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns>True, False or the default value.</returns>
        public bool GetPlayerActivation(string player) => activations.ContainsKey(player) ? activations[player] : DefaultActivation;

        #endregion

        #region Constructor

        public Passive()
        {
            Exports.Add("setPlayerActivation", new Func<int, bool, bool>(SetPlayerActivation));
        }

        #endregion

        #region Export

        /// <summary>
        /// Sets the Passive Mode activation of a player.
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="activation">The new activation status.</param>
        public bool SetPlayerActivation(int id, bool activation)
        {
            // Try to get the player
            Player player = Players[id];
            // If is not valid, return
            if (player == null)
            {
                return false;
            }

            // Otherwise, save the new activation
            activations[player.Handle] = activation;
            // And send it to everyone
            TriggerClientEvent("simplepassive:activationChanged", player, activation);
            Debug.WriteLine($"Passive Activation of '{player.Name}' ({player.Handle}) is now {activation}");
            return true;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command that toggles the passive mode activation of the player.
        /// </summary>
        [Command("togglepassive")]
        public void TogglePassiveCommand(int source, List<object> arguments, string raw)
        {
            // If the source is the Console or RCON, return
            if (source < 1)
            {
                Debug.WriteLine("This command can only be used by players on the server");
                return;
            }

            // Convert the source to a string
            string src = source.ToString();
            // If the player is allowed to change the activation of itself
            if (API.IsPlayerAceAllowed(src, "simplepassive.changeself"))
            {
                // Get the activation of the player, but inverted
                bool oposite = !GetPlayerActivation(src);
                // Save it and send it to everyone
                activations[src] = oposite;
                TriggerClientEvent("simplepassive:activationChanged", src, oposite);
                Debug.WriteLine($"Player {source} set it's activation to {oposite}");
            }
        }

        #endregion
    }
}
