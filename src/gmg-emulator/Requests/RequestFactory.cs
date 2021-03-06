﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Gmg.Emulator.Requests
{
    public static class RequestFactory
    {
        private const string GRILL_INFO = "UR001!";
        private const string SET_GRILL_TEMP = @"UT(\d{3})!";
        private const string SET_PROBE_TEMP = @"UF(\d{3})!";
        private const string POWER_ON = "UK001!";
        private const string POWER_OFF = "UK004!";
        private const string GRILL_ID = "UL!";
        private const string GRILL_FIRMWARE = "UN!";
        private const string TODO = "UA!";

        private static readonly Dictionary<string, Func<Match, IRequest>> FactoryMaps =
            new Dictionary<string, Func<Match, IRequest>>
            {
                {POWER_ON, code => new PowerOnRequest(code.Value)},
                {POWER_OFF, code => new PowerOffRequest(code.Value)},
                {GRILL_INFO, code => new GrillInfoRequest(code.Value)},
                {GRILL_ID, code => new GrillIdRequest(code.Value)},
                {GRILL_FIRMWARE, code => new GrillFirmwareRequest(code.Value)},
                {SET_GRILL_TEMP, ParseGetGrillTemp},
                {SET_PROBE_TEMP, ParseGetProbeTemp}
            };

        public static IRequest CreateFromBytes(byte[] commandCodeBytes)
        {
            if (commandCodeBytes == null) throw new ArgumentNullException(nameof(commandCodeBytes));

            var commandCode = Encoding.ASCII.GetString(commandCodeBytes);
            foreach (var map in FactoryMaps)
            {
                var pattern = map.Key;
                var factory = map.Value;
                var match = Regex.Match(commandCode, pattern);
                if (!match.Success) continue;

                var cmd = factory.Invoke(match);
                return cmd;
            }

            return null;
        }

        private static IRequest ParseGetGrillTemp(Match codeMatch)
        {
            var tempData = codeMatch.Groups[1].Value;
            var temp = ushort.Parse(tempData);
            return new SetGrillTempRequest(codeMatch.Value, temp);
        }

        private static IRequest ParseGetProbeTemp(Match codeMatch)
        {
            var tempData = codeMatch.Groups[1].Value;
            var temp = ushort.Parse(tempData);
            return new SetProbeTempRequest(codeMatch.Value, temp);
        }
    }
}