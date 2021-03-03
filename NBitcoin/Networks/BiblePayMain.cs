using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Crypto;
using NBitcoin.Protocol;

namespace NBitcoin.Networks
{


    public class DashMain : Network
    {

        /// <summary> Stratis maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int StratisMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Stratis default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int StratisDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The default name used for the Stratis configuration file. </summary>
        public const string StratisDefaultConfigFilename = "dash.conf";

        private static Block CreateBiblepayGenesisBlock(ConsensusFactory consensusFactory, string pszTimestamp, uint nTime,
    uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            Transaction txNew = new Transaction();
            txNew.Version = 1;

            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(486604799), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)4 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(pszTimestamp)))
            });

            Script genesisOutputScript = new Script(Op.GetPushOp(Encoders.Hex.DecodeData("040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9")), OpcodeType.OP_CHECKSIG);

            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
                ScriptPubKey = genesisOutputScript
            });

            byte[] b = new byte[1];
            b[0] = 0;
            txNew.Outputs[0].sTxOutMessage = b;
            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(nTime);
            genesis.Header.Bits = nBits;
            genesis.Header.Nonce = nNonce;
            genesis.Header.Version = 1;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();

            return genesis;
        }

        private static Block CreateBiblepayGenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            string pszTimestamp = "Pray always that ye may be accounted worthy to stand before the Son of Man.";

            return CreateBiblepayGenesisBlock(consensusFactory, pszTimestamp, nTime, nNonce, nBits, nVersion, genesisReward);
        }


        public DashMain()
        {
            var messageStart = new byte[4];
            messageStart[0] = 0xbf;
            messageStart[1] = 0x0c;
            messageStart[2] = 0x6b;
            messageStart[3] = 0xbd;
            var magic = BitConverter.ToUInt32(messageStart, 0); //0x5223570; 

            this.Name = "DashMain";
            this.Magic = magic;

            this.DefaultPort = 40000;
            this.RPCPort = 16174;

            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = 10000;
            this.FallbackFee = 60000;
            this.MinRelayTxFee = 10000;
            this.DefaultConfigFilename = StratisDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "DASH";
            this.Consensus.SubsidyHalvingInterval = 210000;

            Consensus.MajorityEnforceBlockUpgrade = 750;
            Consensus.MajorityRejectBlockOutdated = 950;
            Consensus.MajorityWindow = 1000;
            Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 227931;
            Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 388381;
            Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 363725;
            Consensus.BIP34Hash = new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8");
            Consensus.PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            Consensus.PowTargetTimespan = TimeSpan.FromSeconds(7 * 60 * 60); // two weeks
            Consensus.PowTargetSpacing = TimeSpan.FromSeconds(7 * 60);
            Consensus.PowAllowMinDifficultyBlocks = false;
            Consensus.PowNoRetargeting = false;
            //Block.BlockSignature = false;
            this.Consensus.PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));

            Consensus.RuleChangeActivationThreshold = 1916; // 95% of 2016
            Consensus.MinerConfirmationWindow = 2016; // nPowTargetTimespan / nPowTargetSpacing

            Consensus.BIP9Deployments[BIP9Deployments.TestDummy] = new BIP9DeploymentsParameters(28, 1199145601, 1230767999);
            Consensus.BIP9Deployments[BIP9Deployments.CSV] = new BIP9DeploymentsParameters(0, 1462060800, 1493596800);
            Consensus.BIP9Deployments[BIP9Deployments.Segwit] = new BIP9DeploymentsParameters(1, 0, 0);
            Consensus.CoinType = 0;
            Consensus.DefaultAssumeValid = new uint256("0x8c2cf95f9ca72e13c8c4cdf15c2d7cc49993946fb49be4be147e106d502f1869"); // 642930

            this.Consensus.PowTargetTimespan = TimeSpan.FromSeconds(14 * 24 * 60 * 60); // two weeks
            this.Consensus.PowTargetSpacing = TimeSpan.FromSeconds(7 * 60);
            this.Consensus.PowAllowMinDifficultyBlocks = false;
            this.Consensus.PowNoRetargeting = false;
            this.Consensus.MinerConfirmationWindow = 2016; // nPowTargetTimespan / nPowTargetSpacing
            this.Consensus.IsProofOfStake = false;

            this.Consensus.CoinbaseMaturity = 100;
            this.Consensus.PremineReward = Money.Coins(0);
            this.Consensus.PremineHeight = 0;
            this.Consensus.MaxReorgLength = 0;
            this.Consensus.MaxMoney = Money.Coins(50000000);

            this.Consensus.ProofOfWorkReward = Money.Coins(1000000);
            this.Consensus.MaxReorgLength = 100;
            this.Consensus.ProofOfWorkReward = Money.Coins(95950000);
            this.Consensus.ProofOfStakeReward = Money.Zero;

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (76) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (16) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (204) };
            //this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (128+25) };

            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };

            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };


            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 05 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x05 };

            var encoder = new Bech32Encoder("dash");
            this.Bech32Encoders = new Bech32Encoder[2];
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x3b4431310395638c0ed65b40ede4b110d8da70fcc0c2ed4a729fb8e4d78b4452"),
                new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }
            };

                       Block genesis = CreateBiblepayGenesisBlock(this.Consensus.ConsensusFactory, 1496347844, 12, 0x207fffff, 1, Money.COIN * 50);
                      this.Genesis = genesis;
                   this.Consensus.HashGenesisBlock = this.Genesis.GetHash();
            return;

        }

    }

    public class BiblepayTest : Network
    {
        /// <summary> Stratis maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int StratisMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Stratis default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int StratisDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different Stratis blockchains (StratisMain, StratisTest, StratisRegTest). </summary>
        public const string StratisRootFolderName = "BiblepayTest";

        /// <summary> The default name used for the Stratis configuration file. </summary>
        public const string StratisDefaultConfigFilename = "biblepaytest.conf";


        public BiblepayTest()
        {
            var messageStart = new byte[4];
            messageStart[0] = 0xbf;
            messageStart[1] = 0x0c;
            messageStart[2] = 0x6b;
            messageStart[3] = 0xbd;
            var magic = BitConverter.ToUInt32(messageStart, 0); //0x5223570; 

            this.Name = "BiblepayTest";
            this.Magic = magic;

            this.DefaultPort = 40001;
            this.RPCPort = 16174;

            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = 10000;
            this.FallbackFee = 60000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = StratisRootFolderName;
            this.DefaultConfigFilename = StratisDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "tBBP";
            this.Consensus.SubsidyHalvingInterval = 210000;

            Consensus.MajorityEnforceBlockUpgrade = 750;
            Consensus.MajorityRejectBlockOutdated = 950;
            Consensus.MajorityWindow = 1000;
            Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 227931;
            Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 388381;
            Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 363725;
            Consensus.BIP34Hash = new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8");
            Consensus.PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            Consensus.PowTargetTimespan = TimeSpan.FromSeconds(7 * 60 * 60); // two weeks
            Consensus.PowTargetSpacing = TimeSpan.FromSeconds(7 * 60);
            Consensus.PowAllowMinDifficultyBlocks = false;
            Consensus.PowNoRetargeting = false;
            //Block.BlockSignature = false;
            this.Consensus.PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));

            Consensus.RuleChangeActivationThreshold = 1916; // 95% of 2016
            Consensus.MinerConfirmationWindow = 2016; // nPowTargetTimespan / nPowTargetSpacing

            Consensus.BIP9Deployments[BIP9Deployments.TestDummy] = new BIP9DeploymentsParameters(28, 1199145601, 1230767999);
            Consensus.BIP9Deployments[BIP9Deployments.CSV] = new BIP9DeploymentsParameters(0, 1462060800, 1493596800);
            Consensus.BIP9Deployments[BIP9Deployments.Segwit] = new BIP9DeploymentsParameters(1, 0, 0);
            Consensus.CoinType = 0;
            Consensus.DefaultAssumeValid = new uint256("0x8c2cf95f9ca72e13c8c4cdf15c2d7cc49993946fb49be4be147e106d502f1869"); // 642930

            this.Consensus.PowTargetTimespan = TimeSpan.FromSeconds(14 * 24 * 60 * 60); // two weeks
            this.Consensus.PowTargetSpacing = TimeSpan.FromSeconds(7 * 60);
            this.Consensus.PowAllowMinDifficultyBlocks = false;
            this.Consensus.PowNoRetargeting = false;
            this.Consensus.MinerConfirmationWindow = 2016; // nPowTargetTimespan / nPowTargetSpacing
            this.Consensus.IsProofOfStake = false;

            this.Consensus.CoinbaseMaturity = 100;
            this.Consensus.PremineReward = Money.Coins(0);
            this.Consensus.PremineHeight = 0;
            this.Consensus.MaxReorgLength = 0;
            this.Consensus.MaxMoney = Money.Coins(50000000);

            this.Consensus.ProofOfWorkReward = Money.Coins(1000000);
            this.Consensus.MaxReorgLength = 100;
            this.Consensus.ProofOfWorkReward = Money.Coins(95950000);
            this.Consensus.ProofOfStakeReward = Money.Zero;

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (140) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (19) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (239) };
    
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };

            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };


            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 05 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x05 };

            var encoder = new Bech32Encoder("dash");
            this.Bech32Encoders = new Bech32Encoder[2];
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x3b4431310395638c0ed65b40ede4b110d8da70fcc0c2ed4a729fb8e4d78b4452"),
                new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }
            };

            Block genesis = CreateBiblepayGenesisBlock(this.Consensus.ConsensusFactory, 1496347844, 12, 0x207fffff, 1, Money.COIN * 50);
            this.Genesis = genesis;
            this.Consensus.HashGenesisBlock = this.Genesis.GetHash();
            // Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x3b4431310395638c0ed65b40ede4b110d8da70fcc0c2ed4a729fb8e4d78b4452"));
            // Assert(genesis.Header.HashMerkleRoot == uint256.Parse("0x02b05f3b8a7168bcf83b888e0092446b248b2641bd9844b5d12a45eaa2765725"));
            // Default BiblePay Port=40000, TestNet = 40001, RPC=Set_by_user

            this.DNSSeeds = new List<DNSSeedData>
            {
                    new DNSSeedData("dns2.biblepay.org", "node.biblepay.org"),
                    new DNSSeedData("seednode4.loud", "seednode4.loud")
            };

            string[] seedNodes = { "101.200.198.155", "103.24.76.21", "104.172.24.79" };
            this.SeedNodes = ConvertToNetworkAddresses(seedNodes, this.DefaultPort).ToList();
        }

        private static Block CreateBiblepayGenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            string pszTimestamp = "Pray always that ye may be accounted worthy to stand before the Son of Man.";

            return CreateBiblepayGenesisBlock(consensusFactory, pszTimestamp, nTime, nNonce, nBits, nVersion, genesisReward);
        }

        private static Block CreateBiblepayGenesisBlock(ConsensusFactory consensusFactory, string pszTimestamp, uint nTime,
            uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            Transaction txNew = new Transaction();
            txNew.Version = 1;

            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(486604799), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)4 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(pszTimestamp)))
            });

            Script genesisOutputScript = new Script(Op.GetPushOp(Encoders.Hex.DecodeData("040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9")), OpcodeType.OP_CHECKSIG);

            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
                ScriptPubKey = genesisOutputScript
            });

            byte[] b = new byte[1];
            b[0] = 0;
            txNew.Outputs[0].sTxOutMessage = b;
            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(nTime);
            genesis.Header.Bits = nBits;
            genesis.Header.Nonce = nNonce;
            genesis.Header.Version = 1;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();

            return genesis;
        }
    }


    public class BiblepayMain : Network
    {
        /// <summary> Stratis maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int StratisMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Stratis default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int StratisDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different Stratis blockchains (StratisMain, StratisTest, StratisRegTest). </summary>
        public const string StratisRootFolderName = "BiblepayMain";

        /// <summary> The default name used for the Stratis configuration file. </summary>
        public const string StratisDefaultConfigFilename = "biblepay.conf";


        public BiblepayMain()
        {
            var messageStart = new byte[4];
            messageStart[0] = 0xbf;
            messageStart[1] = 0x0c;
            messageStart[2] = 0x6b;
            messageStart[3] = 0xbd;
            var magic = BitConverter.ToUInt32(messageStart, 0); //0x5223570; 

            this.Name = "BiblepayMain";
            this.Magic = magic;

            this.DefaultPort = 40000;
            this.RPCPort = 16174;

            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = 10000;
            this.FallbackFee = 60000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = StratisRootFolderName;
            this.DefaultConfigFilename = StratisDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "BBP";
            this.Consensus.SubsidyHalvingInterval = 210000;

            Consensus.MajorityEnforceBlockUpgrade = 750;
            Consensus.MajorityRejectBlockOutdated = 950;
            Consensus.MajorityWindow = 1000;
            Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 227931;
            Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 388381;
            Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 363725;
            Consensus.BIP34Hash = new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8");
            Consensus.PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            Consensus.PowTargetTimespan = TimeSpan.FromSeconds(7 * 60 * 60); // two weeks
            Consensus.PowTargetSpacing = TimeSpan.FromSeconds(7 * 60);
            Consensus.PowAllowMinDifficultyBlocks = false;
            Consensus.PowNoRetargeting = false;
            //Block.BlockSignature = false;
            this.Consensus.PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));

            Consensus.RuleChangeActivationThreshold = 1916; // 95% of 2016
            Consensus.MinerConfirmationWindow = 2016; // nPowTargetTimespan / nPowTargetSpacing

            Consensus.BIP9Deployments[BIP9Deployments.TestDummy] = new BIP9DeploymentsParameters(28, 1199145601, 1230767999);
            Consensus.BIP9Deployments[BIP9Deployments.CSV] = new BIP9DeploymentsParameters(0, 1462060800, 1493596800);
            Consensus.BIP9Deployments[BIP9Deployments.Segwit] = new BIP9DeploymentsParameters(1, 0, 0);
            Consensus.CoinType = 0;
            Consensus.DefaultAssumeValid = new uint256("0x8c2cf95f9ca72e13c8c4cdf15c2d7cc49993946fb49be4be147e106d502f1869"); // 642930

            this.Consensus.PowTargetTimespan = TimeSpan.FromSeconds(14 * 24 * 60 * 60); // two weeks
            this.Consensus.PowTargetSpacing = TimeSpan.FromSeconds(7 * 60);
            this.Consensus.PowAllowMinDifficultyBlocks = false;
            this.Consensus.PowNoRetargeting = false;
            this.Consensus.MinerConfirmationWindow = 2016; // nPowTargetTimespan / nPowTargetSpacing
            this.Consensus.IsProofOfStake = false;

            this.Consensus.CoinbaseMaturity = 100;
            this.Consensus.PremineReward = Money.Coins(0);
            this.Consensus.PremineHeight = 0;
            this.Consensus.MaxReorgLength = 0;
            this.Consensus.MaxMoney = Money.Coins(50000000);

            this.Consensus.ProofOfWorkReward = Money.Coins(1000000);
            this.Consensus.MaxReorgLength = 100;
            this.Consensus.ProofOfWorkReward = Money.Coins(95950000);
            this.Consensus.ProofOfStakeReward = Money.Zero;

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (25) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (16) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (182) };
            //this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (128+25) };
            // CRITICAL R ANDREWS 8-30-2019
            //pubkey+ 128 = sec key

            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };

            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };


            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 05 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x05 };

            var encoder = new Bech32Encoder("dash");
            this.Bech32Encoders = new Bech32Encoder[2];
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x3b4431310395638c0ed65b40ede4b110d8da70fcc0c2ed4a729fb8e4d78b4452"),
                new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }
            };

            // BIBLEPAY - FUTURE SEGWIT SUPPORT HERE
            /*
            var encoder = new Bech32Encoder("bc");
            this.Bech32Encoders = new Bech32Encoder[2];
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;
            */

            Block genesis = CreateBiblepayGenesisBlock(this.Consensus.ConsensusFactory, 1496347844, 12, 0x207fffff, 1, Money.COIN * 50);
            this.Genesis = genesis;
            this.Consensus.HashGenesisBlock = this.Genesis.GetHash();
            return;

            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x3b4431310395638c0ed65b40ede4b110d8da70fcc0c2ed4a729fb8e4d78b4452"));
            Assert(genesis.Header.HashMerkleRoot == uint256.Parse("0x02b05f3b8a7168bcf83b888e0092446b248b2641bd9844b5d12a45eaa2765725"));
            // Default BiblePay Port=40000, TestNet = 40001, RPC=Set_by_user

            this.DNSSeeds = new List<DNSSeedData>
            {
                    new DNSSeedData("dns2.biblepay.org", "node.biblepay.org"),
                    new DNSSeedData("seednode4.loud", "seednode4.loud")
            };

            string[] seedNodes = { "101.200.198.155", "103.24.76.21", "104.172.24.79" };
            this.SeedNodes = ConvertToNetworkAddresses(seedNodes, this.DefaultPort).ToList();
        }







        private static void BiblePayTestBed()
        {
            string sBiblePay = "biblepay";
            // Note; The SerializeHash function uses Length+Data, The sha256 however hashes the data + char(0).
            uint256 a1 = Script.SerializeHash(sBiblePay);
            string a2 = GetPartialBibleHash();
            string sMyTest = "";
        }


        public static string GetPartialBibleHash()
        {
            var k = new KJV();
            string myTest = k.b[1];
            string myHi = k.b[2];
            bool f7000 = false;
            bool f8000 = false;
            bool f9000 = false;
            bool fTitheBlocksActive = false;
            int nTime = 3;
            int nPrevTime = 2;
            int nPrevHeight = 1;
            int nNonce = 10;
            var cBible = new BibleHash();
            bool bMining = true;
            string sMGP = cBible.GetMd5String("1234");
            uint256 inHash = new uint256("0000000000000000000000000000000000000000000000000000000000001234");
            uint256 uBibleHash = cBible.GetBibleHash(inHash, nTime, nPrevTime, bMining, nPrevHeight, f7000, f8000, f9000, fTitheBlocksActive, nNonce);
            string sBiblePay = "biblepay";
            string sSha = cBible.GetSha256String(sBiblePay);
            return sSha;
        }



        private static Block CreateBiblepayGenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            string pszTimestamp = "Pray always that ye may be accounted worthy to stand before the Son of Man.";

            return CreateBiblepayGenesisBlock(consensusFactory, pszTimestamp, nTime, nNonce, nBits, nVersion, genesisReward);
        }

        private static Block CreateBiblepayGenesisBlock(ConsensusFactory consensusFactory, string pszTimestamp, uint nTime,
            uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            Transaction txNew = new Transaction();
            txNew.Version = 1;

            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(486604799), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)4 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(pszTimestamp)))
            });

            Script genesisOutputScript = new Script(Op.GetPushOp(Encoders.Hex.DecodeData("040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9")), OpcodeType.OP_CHECKSIG);

            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
                ScriptPubKey = genesisOutputScript
            });

            byte[] b = new byte[1];
            b[0] = 0;
            txNew.Outputs[0].sTxOutMessage = b;
            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(nTime);
            genesis.Header.Bits = nBits;
            genesis.Header.Nonce = nNonce;
            genesis.Header.Version = 1;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();

            BiblePayTestBed();

            return genesis;
        }


    }

}
