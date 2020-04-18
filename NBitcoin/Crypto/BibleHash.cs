using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NBitcoin.DataEncoders;

namespace NBitcoin.Crypto
{
    public class BibleHash
    {
        public string[] book = new string[66];
        public string[] sins = new string[40];
        public string[] prayer = new string[6];
        double iVerseFactor = .4745708; //Verses available divided by bits per octet
        public const int SPORK8_HEIGHT = 23000;
        public bool fDistributedComputingEnabled = true;
        public string sChainedVerses = "";
        public KJV kjv = new KJV();
        public bool fProd = true;
        public const int F11000_CUTOVER_HEIGHT_PROD = 33440;
        public const int TITHE_MODULUS = 10;
        public const int F10000_CUTOVER_HEIGHT = 25910;

        public BibleHash()
        {
            // The 66 books of the bible:
            book[0] = "Gen|Genesis";
            book[1] = "Exo|Exodus";
            book[2] = "Lev|Leviticus";
            book[3] = "Num|Numbers";
            book[4] = "Deu|Deuteronomy";
            book[5] = "Jos|Joshua";
            book[6] = "Jdg|Judges";
            book[7] = "Rut|Ruth";
            book[8] = "Sa1|1 Samuel";
            book[9] = "Sa2|2 Samuel";
            book[10] = "Kg1|Kings 1";
            book[11] = "Kg2|Kings 2";
            book[12] = "Ch1|1 Chronicles";
            book[13] = "Ch2|2 Chronicles";
            book[14] = "Ezr|Ezra";
            book[15] = "Neh|Nehemiah";
            book[16] = "Est|Esther";
            book[17] = "Job|Job";
            book[18] = "Psa|Psalms";
            book[19] = "Pro|Proverbs";
            book[20] = "Ecc|Ecclesiastes";
            book[21] = "Sol|Song of Solomon";
            book[22] = "Isa|Isaiah";
            book[23] = "Jer|Jeremiah";
            book[24] = "Lam|Lamentations";
            book[25] = "Eze|Ezekiel";
            book[26] = "Dan|Daniel";
            book[27] = "Hos|Hosea";
            book[28] = "Joe|Joel";
            book[29] = "Amo|Amos";
            book[30] = "Oba|Obadiah";
            book[31] = "Jon|Jonah";
            book[32] = "Mic|Micah";
            book[33] = "Nah|Nahum";
            book[34] = "Hab|Habakkuk";
            book[35] = "Zep|Zephaniah";
            book[36] = "Hag|Haggai";
            book[37] = "Zec|Zechariah";
            book[38] = "Mal|Malachi";
            book[39] = "Mat|Matthew";
            book[40] = "Mar|Mark";
            book[41] = "Luk|Luke";
            book[42] = "Joh|John";
            book[43] = "Act|Acts";
            book[44] = "Pau|1 Paul to the Romans";
            book[45] = "Co1|1 Corinthians";
            book[46] = "Co2|2 Corinthians";
            book[47] = "Gal|Galatians";
            book[48] = "Eph|Ephesians";
            book[49] = "Phi|Phillipians";
            book[50] = "Col|Colossians";
            book[51] = "Th1|1 Thessalonians";
            book[52] = "Th2|2 Thessalonians";
            book[53] = "Ti1|1 Timothy";
            book[54] = "Ti2|2 Timothy";
            book[55] = "Tit|Titus";
            book[56] = "Plm|Philemon";
            book[57] = "Heb|Hebrews";
            book[58] = "Jam|James";
            book[59] = "Pe1|1 Peter";
            book[60] = "Pe2|2 Peter";
            book[61] = "Jo1|1 John";
            book[62] = "Jo2|2 John";
            book[63] = "Jo3|3 John";
            book[64] = "Jde|Jude";
            book[65] = "Rev|Revelation";


            // 39 SINS THAT CAN SEND YOU TO HELL (IF YOU DO NOT REPENT)
            sins[0] = "IDOLATRY|The making of any created thing to be your 'god'; Setting your ultimate love upon, finding your ultimate security in, or giving your ultimate allegiance to any created thing(s), whether material, human, or formal false religious gods. Attributing uniquely divine attributes to created entities or things; worshiping such as God. Idolatry is a sin of atheism as by necessity it effectively makes gods out of created things. (Is. 45:18; Ex. 20:3 Dt. 5:7; 6:5,14; 17:2-7; 27:15; Acts 21:25; 1 Cor. 5:11; 12:2; 2 Cor. 6:16; 1 Thes. 1:9; 1 Jn. 5:21; Rev. 2:14,20; 9:20)";
            sins[1] = "BLASPHEMY|Claiming to be God, or attacking/disrespecting God and his character, or charging God with being immoral, or preaching a misconstrued (altered) version of the Gospel. Claiming to have superior wisdom and judgment than God. (Lv. 24:16; 1Ki. 21:10; Mt. 12:31; Acts 26:11; James 2:7)";
            sins[2] = "TAKING THE NAME OF GOD IN VAIN|Misusing or misappropriating God's name and authority for any purpose, such as invoking the name of God/Christ to give authority to the unholy curses or purposes of man. Includes falsely speaking prophecy in the name of the LORD. It is a form of the sin of profanity. (Lv. 19:8,12; Dt. 18:20a; 20:7; Jer. 20:1-6; 28:1,2,10-17;29:24-32)";
            sins[3] = "PROFANITY|Irreverence or being contemptuous toward what God calls holy, sanctified, in word or deed. (Gn. 18:19; Ezra 2:62; 9:2; Neh. 13:23-29; Ezek. 23:26; Mal. 2:11,15; Rm. 11:16; 1Co 7:14)";
            sins[4] = "BEING A FALSE PREACHER|One who entices others to practice false religion, to go after other gods; apostasy, or which teach contrary to salvific Scriptural Truths. (Dt. 13:6-12; Mt. 23:15; Acts 13:10; 2Cor. 11:13-15) Akin to the sin of misdirecting people, from making the blind wander out of the way to maliciously giving false directions. (Dt. 27:18; Mt. 15:14)";
            sins[5] = "WITCHCRAFT|The practice of witches; occultic magic: Ouija boards, astrology, palmistry, necromancy, divination, etc.; also likened to rebellion. (Ex. 22:18; Lv. 19:31; 20:6,27; 1Sam. 15:23)";
            sins[6] = "CHILD SACRIFICE TO IDOLS, FALSE GODS|basically includes abortion (most of which are done for convenience), or directing them to laws and ideologies contrary to God's word, and which directs them on the path of damnation. (Lv. 18:21; 20:2; Dt. 12:31; 18:10)";
            sins[7] = "REBELLION AGAINST PARENTS|Cursing, disrespect, or constant rebellion against parents (except where obedience is required due to conflict with God's word) (Ex. 20:12; 21:15, 17; cf. Lv. 20:9; Dt. 21:18; 27:16)";
            sins[8] = "DISOBEDIENCE TO AUTHORITY|Disobedience to just judgments by God-ordained authority. (Dt. 17:9,10a, 11b,12; Josh 1:18; Rm. 13:1-7; 1Pet. 2:13,14)";
            sins[9] = "MURDER|Premeditated killing with malice; intentional unlawful and or unjust killing of another human; homicide with malicious forethought. (Ex. 21:12-14: Lv. 24:17; Num. 35:31; Dt. 19:11,12; Jn. 8:44; 1 Tim. 1:9; 1 Pet. 4:15; 1 Jn. 3:15) Includes murdering one in their thoughts. (Mk. 7:21)";
            sins[10] = "NEGLIGENT HOMICIDE|Death of another caused by one's own negligence. (Ex. 21:29)";
            sins[11] = "HOMOSEXUAL RELATIONS|Sexual activity between persons of the same gender; seeking to join same genders in marriage (cf. here and here (Lv. 18:22; 20:13; Rm. 1:26,27; 1 Tim. 1:10)";
            sins[12] = "EFFEMINANCY|Men unnaturally taking on female characteristics and behavior, contrary to the distinctive nature of opposite genders God created, and thus their complimentarity. (1 Cor. 6:9; cf. Dt. 22:5)";
            sins[13] = "BESTIALITY|Sexual relations between humans and animals. (Ex. 22:19; Lv. 18:23; 20:15; Dt. 27:21)";
            sins[14] = "ADULTERY|Violation of the marriage bed; extramarital sexual relations in heart or actions (includes lusting by pornography). (Lv. 20:10; Dt. 22:23-25; Mk. 7:20-23; Jn. 8:3-5; Gal. 5:19; 1 Cor. 6:9)";
            sins[15] = "FORNICATION|Sexual intercourse before marriage: also adultery and spiritual unfaithfulness. (Gn. 34; Mt. 19:9; Mk. 7:20-23; Acts 15:20; 21:25; Rm. 1:29; 1 Cor. 5:1,11; 6:13,19; 7:2; Gal. 5:19; Eph. 5:3; Col. 3:5; 1Thes. 4:3; Rev. 9:21; 14:8; 17:2,4; 18:3; 19:2);";
            sins[16] = "SEXUAL UNCLEANNESS|All unnatural, impure or otherwise illicit (such as unmarried) sexual activity, in heart or deed. (Mk. 7:20-23; 2Cor. 12:21; Gal. 5:19; Eph. 5:3; Col. 3:5)";
            sins[17] = "DECEIT, LYING|Dishonesty in every form; being a false witness. (Dt. 19:15; Mk. 7:22; 14:1; Acts 13:10; Rm. 1:29; 1 Tim. 1:10; 1Thes. 2:3; 1Pet. 2:1,22; 3:10; Rev. 2:2; 14:5; 21:8)";
            sins[18] = "FALSE PRETENSE IN MARRIAGE|Marriage under the pretense of being a virgin. (Dt. 22:20)";
            sins[19] = "LASCIVIOUSNESS|Lustful, wanton; captivating lust. (Gal. 5:19; Eph. 4:19; 1Pet. 4:3; Jude 1:4)";
            sins[20] = "COVETOUSNESS|Selfish lusting for things, whether pleasure (food, sex), possessions or power/prestige. (Lk. 12:15; Rm. 1:29; 2 Cor. 9:5; Eph. 4:19; 5:3; Col. 3:5; 1 Thes. 2:5; 2 Pet. 2:3,14; 1Jn. 2:16)";
            sins[21] = "THEFTS|Stealing; the unlawful taking of anything material, intellectual, etc. (Ex. 20:15; Lv. 19:11; Dt. 19:14; Dt. 27:17; Ezek. 22:29; Mk. 7:22; Jn. 12:6; Eph. 4:28; 1 Pet. 4:15)";
            sins[22] = "PERVERTING JUSTICE|Perversion of what is right for the stranger, fatherless or widow. (Ex. 22:22-24; Dt. 27:19)";
            sins[23] = "HATRED|Unholy bitter aversion, animosity, antagonism, resentment. (Rm. 1:29; Gal. 5:20; James 4:4)";
            sins[24] = "VARIANCE|Contentious spirit; refusal to submit to truth, or just rulings by lawful authority. (Rm. 1:29; 1Cor. 3:3; Gal. 5:20; Titus 3:9; Dt. 17:8-13)";
            sins[25] = "EMULATIONS|Unholy zeal; selfish ambition, strife to excel at the expense of another. (Num. 12; Gal. 5:20; Phil. 2:14)";
            sins[26] = "WRATH|Carnal turbulent, violent passions of unholy anger, indignation. (2 Cor. 12:20; Gal. 5:20; Col. 3:8)";
            sins[27] = "STRIFE|Fleshly disputations; selfish contention for superiority. (Gal. 5:20; Rm. 1:29; 13:13; 1 Cor. 1:11; 2 Cor. 12:20; Phil. 1:15; 2:24; 1Tim. 6:4; 3:9; 1 Pet. 5:5)";
            sins[28] = "SEDITION|Carnal assembly of rebellious spirit in opposition to lawful authority: fleshly factious uprising; (Num. 12; 16; Rm. 16:7; 1 Cor. 3:3; Gal. 5:20)";
            sins[29] = "HERESIES|Unholy factions holding false doctrine opposed to established fundamental truths. (Gal. 5:20; Titus 1:3)";
            sins[30] = "ENVYINGS|Feeling displeasure at the excellence or prosperity of another because you feel you should have it also. A cousin to jealousy, selfishly feeling what others have belongs to you instead. (Num. 16:3,7; Mt. 27:18; Mk. 15:10; Rm. 1:29; Phil. 1:15; 1Tim. 6:4; Titus 3:3; 1Pet. 2:1)";
            sins[31] = "DRUNKENNESS|The state of being drunk; under the influence of intoxicating substances which alter the senses for recreation. Even the cares of the world, when they intoxicate the mind (Adam Clarke). (Ga. 5:21; Dt. 21:20; Lk. 21:34; 1 Cor. 5:11; 6:10; Eph. 5:18; 1 Thes. 5:7)";
            sins[32] = "REVELINGS|Boisterous, indulgent festivities, carousing, loud merry-making, partying. (Ex. 32:1-8; Gal. 5:21; Rm. 13:13; 1Pet. 4:3)";
            sins[33] = "PRIDE|Haughtiness; egotism; conceit. Seeing yourself better than you really are. (Ps. 12:3; 101:5; 119:21; 123:4; 138:6; Prv. 6:17; 15:25; 16:5; 21:4,24; 28:25; Eccl. 7:8; Is. 2:12)";
            sins[34] = "FOOLISHNESS|Frivolous or irresponsible behavior. (Prv. 12:23; 22:15; 24:9; 27:2; Eccl. 7:25; 1 Pet. 2:15)";
            sins[35] = "EXTORTION|Forced extraction of money, sex, things, by manipulation; coercion. (1 Cor. 5:11; Lk. 18:11; Mt. 7:15)";
            sins[36] = "REFUSING TO FORGIVE|Holding a grudge,harboring personal resentment, a bitter spirit against persons due to past injuries (see Hurt and Resentment); pressing charges in heart, especially despite ignorance or repentance on the other person's part. (Lv. 19:18; Mt. 6:12-15; 18:23-35; Heb. 12:15) This does not exclude bringing issues to a person or the church that should be addressed as being wrong in principle, and in the interest of peace and righteousness, (cf. Mt. 18:15-17) but a forgiving spirit, not a resentful one, is required in the light of God's forgiveness of us, and not seeking satisfaction for injuries is the higher standard. (Lk. 23:34; Acts 7:50; 1Co. 6:7)";
            sins[37] = "EVIL THOUGHTS|Any evil thoughts, or imaginations, including unwarranted suspicions; seeking vengeance; illicit sex fantasies; planning evil. (Mk. 7:21; Mt. 9:4; James 2:4)";
            sins[38] = "THE ULTIMATE SIN OF DAMNATION|Rejecting the Lord Jesus Christ, the Son of the Living God, and the only One who can save you from all your sins.";

            // The Sinners Prayer
            prayer[0] = "THE SINNERS PRAYER||Father, I know that I have broken your laws and my sins have separated me from you.|I am truly sorry, and now I want to turn away from my past sinful life toward you.|Please forgive me, and help me avoid sinning again.|I believe that your son, Jesus Christ died for my sins, was resurrected from the dead, is alive, and hears my prayer.|I invite Jesus to become the Lord of my life, to rule and reign in my heart from this day forward.|Please send your Holy Spirit to help me obey You, and to do Your will for the rest of my life.|In Jesus' name I pray, Amen.";
            prayer[1] = "THE LORDS PRAYER||Our Father who art in heaven,|hallowed be thy name.|Thy kingdom come,|Thy will be done,|on earth, as it is in heaven.|Give us this day our daily bread,|and forgive us of our trespasses,|as we forgive those that trespass against us.|And lead us not into temptation,|but deliver us from evil.|For thine is the Kingdom and the Power, and The Glory Forever.|Amen";
            prayer[2] = "THE APOSTLES CREED||I believe in God, the Father Almighty,|the Creator of heaven and earth,|and in Jesus Christ, his only Son, our Lord;|Who was conceived by the Holy Spirit,|born of the Virgin Mary,|suffered under Pontius Pilate,|was crucified, died, and was buried.|He descended to the dead.|The third day he arose again from the dead.|He ascended into heaven|and sits at the right hand of God the Father Almighty,|whence he shall come to judge the living and the dead.|I believe in the Holy Spirit,|the holy universal Church,|the communion of saints,|the forgiveness of sins,|the resurrection of the body,|and the life everlasting.|Amen.";
            prayer[3] = "THE NICENE CREED||We believe in one God,|the Father almighty,|maker of heaven and earth,|of all things visible and invisible.|And in one Lord Jesus Christ,|the only Son of God,|begotten from the Father before all ages,|God from God,|Light from Light,|true God from true God,|begotten, not made;|of the same essence as the Father.|Through him all things were made.|For us and for our salvation|he came down from heaven;|he became incarnate by the Holy Spirit and the virgin Mary,|and was made human.|He was crucified for us under Pontius Pilate;|he suffered and was buried.|The third day he rose again, according to the Scriptures.|He ascended to heaven|and is seated at the right hand of the Father.|He will come again with glory|to judge the living and the dead.|His kingdom will never end.|And we believe in the Holy Spirit,|the Lord, the giver of life.|He proceeds from the Father and the Son,|and with the Father and the Son is worshiped and glorified.|He spoke through the prophets.|We believe in one holy catholic and apostolic church.|We affirm one baptism for the forgiveness of sins.|We look forward to the resurrection of the dead,|and to life in the world to come. Amen.";
            prayer[4] = "THE TEN COMMANDMENTS||I am the Lord your God: You shall have no other gods before me|Thou shall not make for yourself an idol|Thou shall not make wrongful use of the name of your God|Remember the Sabbath and keep it holy|Honor your Father and Mother|Thou shall not murder|Thou shall not commit adultery|Thou shall not steal|Thou shall not bear false witness against your neighbor|Thou shall not covet";
            prayer[5] = "JESUS' CONCISE COMMANDMENTS||Place God First: Seek first the kingdom of God, and his righteousness; and all these things shall be added unto you (Matt 6:33 NIV).|Love God above everything else including your Family on Earth.|Have Complete Faith in God|Ask, Seek, and Knock - Ask and it will be given to you; seek and you will find; knock and the door will be opened to you (Matt 7:7 NIV).|Love Each Other as I have loved you (John 15:12 KJV). Love covers a multitude of sins.|Love Your Enemies and pray for those who persecute you, for God causes the sun to rise on the evil and good and rain on unrighteous (Matt 5:43-45 NIV).|You must be a born again Believer in Christ (John 3:7 KJV) - And Enter Through the Narrow Gate|Small and narrow is the road that leads to life, and only a few find it (Matt 7:13-14 NIV).|Forgive others - and Do Not Judge Others: Forgive others so that your Father in heaven may forgive you (Mark 11:25-26 NIV) and do not hold grudges. (Matt 6:9-15 NIV).|Do not Judge - or you too will be judged. (Matt 7:1-2 NIV).|Repent of Your Sins - Repent, for the kingdom of heaven has come near. (Matthew 4:17)|Remain in me, and I will remain in you. No branch can bear fruit by itself; it must remain in the vine. Neither can you bear fruit unless you remain in me. (John 15:4 NIV)|Be Humble and Merciful and Do Not Exalt Yourself: Be humble enough to wash one anothers feet (John 13:14 NIV), and Be merciful, just as your Father is merciful, (Luke 6:36 NIV). |Care for Those in Distress - On judgement day, may God say to you : I was a stranger and you invited me in, I needed clothes and you clothed me, I was sick and you looked after me, I was in prison and you came to visit me.|Give to those who ask and do not turn away from the one who wants to borrow from you (Matt 5:40-42 NIV).|Pure and undefiled religion before our God and Father is to care for orphans and widows in their distress, and to keep oneself from being polluted by the world. (John 1:27)|Let your light shine before men - and Be the salt of the Earth - Let men see your good deeds and praise your Father in heaven (Matt 5:14;16 NIV).|Settle matters quickly with your adversary - between one another (Matt 5:25 NIV) then escalate the issue to one or two brothers, next escalate it to the church, (Matt 18:15-17 NIV)|Get rid of whatever causes you to sin (Matt 5:29-30 NIV)|Exercise Spiritual Power - Spread the Gospel And Reform the Church: We have authority to drive out evil spirits and to heal every disease and sickness (Matt 10:1 NIV).|Heal the sick, raise the dead, cleanse those who have leprosy, drive out demons. Freely you have received, freely give, (Matt 10:8 NIV).|Do not Oppose Other Christian Groups - For whoever is not against us is for us (Mark 9:38-40 NIV).|Go and Make Disciples of All Nations, Baptizing Them - Therefore go and make disciples of all nations, baptizing them in the name of the Father and of the Son and of the Holy Spirit, and teaching them to obey everything I have commanded you.|And surely I am with you always, to the very end of the age, (Matt 28:19-20 NIV). And He said to them, ‘Go into all the world and preach the gospel to every creature.' (Mark 16:15)|Obey What I Command - If you love me, you will obey what I command, (John 14:15 NIV).|Do Not Swear (I swear On My ______) and do not let evil words come out of your mouth (Matt 5:34-37 KJV)|Do Not Repay Evil with Evil - If someone strikes you on the right cheek, turn to him the other also (Matt 5:38-39 NIV).|Give to Charity Privately, Fast inconspicuously, and Pray Privately: Give to Please God, but Not to be Seen by Men - Do not do 'acts of righteousness' before men to be seen by them (Matt 6:1 NIV).|Do not pray like hypocrites on the street corners (Matt 6:5-7 NIV)|Fast Without Fanfare - Do not look somber (Matt 6:16 NIV)|Do not Store up Treasures on Earth where moth and rust destroy, and where thieves break in and steal, but Store up treasures in heaven, For where your treasure is, there your heart will be also |Do not Worry about Your Needs: Arent you much more valuable than birds? (Matt 6:25-26 NIV).|Do not Worry about Tomorrow - Each day has enough trouble of its own (Matt 6:34 NIV).|Partake in Communion in a Clean Manner - Do this in Remembrance of Me : And he took bread, gave thanks and broke it, and gave it to them, saying, This is my body given for you; do this in remembrance of me. (Luke 22:19-20 NIV)|You Must be Ready - You also must be ready, because the Son of Man will come at an hour when you do not expect him (Luke 12:40 NIV).|Watch out for false prophets - They come to you in sheep's clothing, but inwardly they are ferocious wolves, (Matt 7:15 NIV).|Do not Despise the Little Ones - For I tell you that their angels in heaven always see the face of my Father in heaven (Matt 18:10 NIV).";
        }

        public void GetBookStartEnd(string sBook, int iStart, int iEnd)
        {
            sBook = sBook.ToUpper();
            for (int i = 0; i < (int)(kjv.b.Length); i++)
            {
                string sLocalBook = GetArrayElement(kjv.b[i], "|", 0);
                sLocalBook = sLocalBook.ToUpper();
                if (sBook == sLocalBook && iStart == 0) iStart = i;
                if (sBook == sLocalBook && iStart > 0) iEnd = i;
            }
        }

        public static int HexToInteger(string hex)
        {
            int d = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return d;
        }

        double ConvertHexToDouble(string hex)
        {
            int d = HexToInteger(hex);
            double dOut = (double)d;
            return dOut;
        }

        public static double cdbl(string data, int iPlaces)
        {
            double A = Convert.ToDouble(data);
            A = Math.Round(A, iPlaces);
            return A;
        }

        string GetArrayElement(string data, string delimeter, int iPos)
        {
            var vData = data.Split(new string[] { delimeter }, StringSplitOptions.None);
            return vData[iPos];
        }

        string GetVerse(string sBook, int iChapter, int iVerse, int iBookStart, int iBookEnd)
        {
            sBook = sBook.ToUpper();
            for (int i = iBookStart - 1; i <= iBookEnd; i++)
            {
                if (i >= 0)
                {
                    string sLocalBook = GetArrayElement(kjv.b[i], "|", 0);
                    sLocalBook = sLocalBook.ToUpper();
                    int iLocalChapter = (int)cdbl(GetArrayElement(kjv.b[i], "|", 1), 0);
                    int iLocalVerse = (int)cdbl(GetArrayElement(kjv.b[i], "|", 2), 0);
                    if (iChapter == iLocalChapter && iVerse == iLocalVerse && sBook == sLocalBook)
                    {
                        string sVerse = GetArrayElement(kjv.b[i], "|", 3);
                        sVerse = sVerse.Replace("~", "");
                        return sVerse;
                    }
                }
            }
            return "";
        }

        void GetMiningParams(int nHeight, ref bool f7000, ref bool f8000, ref bool f9000, ref bool fTitheBlocksActive)
        {
            f7000 = false;
            f8000 = false;
            f9000 = true;
            fTitheBlocksActive = false;
        }

        string GetBibleHashVerses(uint256 hash, int nBlockTime, int nPrevBlockTime, int nPrevHeight)
        {
            bool f7000 = false;
            bool f8000 = false;
            bool f9000 = false;
            bool fTitheBlocksActive = false;

            GetMiningParams(nPrevHeight, ref f7000, ref f8000, ref f9000, ref fTitheBlocksActive);
            GetBibleHash(hash, nBlockTime, nPrevBlockTime, false, nPrevHeight, f7000, f8000, f9000, fTitheBlocksActive, 0);
            if (sChainedVerses == "") return "";
            string sVerses = sChainedVerses.Replace("~", "");
            return sVerses;
        }


        string GetBibleHashVerseNumber(uint256 hash, int nBlockTime, int nPrevBlockTime, int nPrevHeight, int iVerseNumber)
        {
            string sData = GetBibleHashVerses(hash, nBlockTime, nPrevBlockTime, nPrevHeight);
            var vVerse = sData.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            if (iVerseNumber <= (int)vVerse.Length)
            {
                return vVerse[iVerseNumber];
            }
            else
            {
                return "N/A";
            }
        }


        string GetPrayer(int iPrayerNumber, out string out_Title)
        {
            out_Title = "";
            if (iPrayerNumber < 0 || iPrayerNumber > (int)(prayer.Length - 1)) return "";
            var vPrayer = prayer[iPrayerNumber].Split(new string[] { "|" }, StringSplitOptions.None);
            out_Title = vPrayer[0];
            return prayer[iPrayerNumber];
        }


        string GetSin(int iSinNumber, out string out_Description)
        {
            out_Description = String.Empty;
            if (iSinNumber < 0 || iSinNumber > (int)(sins.Length - 1)) return "";
            var vSin = sins[iSinNumber].Split(new string[] { "|" }, StringSplitOptions.None);
            out_Description = vSin[1];
            return vSin[0];
        }

        string GetBook(int iBookNumber)
        {
            if (iBookNumber < 0 || iBookNumber > (int)(book.Length - 1)) return "";
            var vBook = book[iBookNumber].Split(new string[] { "|" }, StringSplitOptions.None);
            return vBook[1];
        }

        string GetBookByName(string sName)
        {
            sName = sName.ToUpper();
            for (int i = 0; i < (int)(book.Length); i++)
            {
                string sShort = GetArrayElement(book[i], "|", 0);
                string sLong = GetArrayElement(book[i], "|", 1);
                sShort = sShort.ToUpper();
                sLong = sLong.ToUpper();
                if (sShort == sName) return sLong;
                if (sName == sLong) return sShort;
            }
            return String.Empty;
        }

        int ATD(int iAscii)
        {
            int iOut = 0;
            switch (iAscii)
            {
                case 48:
                    iOut = 0;
                    break;
                case 49:
                    iOut = 1;
                    break;
                case 50:
                    iOut = 2;
                    break;
                case 51:
                    iOut = 3;
                    break;
                case 52:
                    iOut = 4;
                    break;
                case 53:
                    iOut = 5;
                    break;
                case 54:
                    iOut = 6;
                    break;
                case 55:
                    iOut = 7;
                    break;
                case 56:
                    iOut = 8;
                    break;
                case 57:
                    iOut = 9;
                    break;
                case 97:
                    iOut = 10;
                    break;
                case 98:
                    iOut = 11;
                    break;
                case 99:
                    iOut = 12;
                    break;
                case 100:
                    iOut = 13;
                    break;
                case 101:
                    iOut = 14;
                    break;
                case 102:
                    iOut = 15;
                    break;
            }
            return iOut;
        }

        public static string ByteArrayToHexString(byte[] arrInput)
        {
            StringBuilder sOutput = new StringBuilder(arrInput.Length);

            for (int i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString().ToLower();
        }

        public string GetSha256String(string sData)
        {
            byte[] arrData = System.Text.Encoding.UTF8.GetBytes(sData);
            var hash = System.Security.Cryptography.SHA256.Create().ComputeHash(arrData);
            return ByteArrayToHexString(hash);
        }

        public string GetShaOfBytes(byte[] bytes)
        {
            var hash = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes);
            return ByteArrayToHexString(hash);
        }

        public string GetMd5String(string sData)
        {
            byte[] arrData = System.Text.Encoding.UTF8.GetBytes(sData);
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(arrData);
            return ByteArrayToHexString(hash);
        }

        static byte[] EncryptStringToBytes(string plainText)
        {
            byte[] bytesText = System.Text.Encoding.ASCII.GetBytes(plainText);
            return EncryptBytes(bytesText);
        }

        private static void DeriveKeyAndIV(byte[] data, byte[] salt, int count, out byte[] key, out byte[] iv)
        {
            List<byte> hashList = new List<byte>();
            byte[] currentHash = new byte[0];

            int preHashLength = data.Length + ((salt != null) ? salt.Length : 0);
            byte[] preHash = new byte[preHashLength];

            System.Buffer.BlockCopy(data, 0, preHash, 0, data.Length);
            if (salt != null)
                System.Buffer.BlockCopy(salt, 0, preHash, data.Length, salt.Length);

            SHA512 hash = SHA512.Create();
            currentHash = hash.ComputeHash(preHash);
            for (int i = 1; i < count; i++)
            {
                currentHash = hash.ComputeHash(currentHash);
            }

            hashList.AddRange(currentHash);

            while (hashList.Count < 48) // for 32-byte key and 16-byte iv
            {
                preHashLength = currentHash.Length + data.Length + ((salt != null) ? salt.Length : 0);
                preHash = new byte[preHashLength];

                System.Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
                System.Buffer.BlockCopy(data, 0, preHash, currentHash.Length, data.Length);
                if (salt != null)
                    System.Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + data.Length, salt.Length);

                currentHash = hash.ComputeHash(preHash);

                for (int i = 1; i < count; i++)
                {
                    currentHash = hash.ComputeHash(currentHash);
                }

                hashList.AddRange(currentHash);
            }
            key = new byte[32];
            iv = new byte[16];
            hashList.CopyTo(0, key, 0, 32);
            hashList.CopyTo(32, iv, 0, 16);
        }

        public static byte[] EncryptBytes2(byte[] byteText)
        {
            var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            string sIV = "1d2c793dedc237897269859b34d31d93";
            string sKey = "3d6b59e8c5623ce4ff7c165995b209e7f03461ec057ca33a5cd1559d01e5682b";
            aes.Key = Encoders.Hex.DecodeData(sKey);
            aes.IV = Encoders.Hex.DecodeData(sIV);
            byte[] encrypted;
            HexEncoder he = new HexEncoder();
            using (MemoryStream ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(byteText, 0, byteText.Length);
                    cs.FlushFinalBlock();
                }
                encrypted = ms.ToArray();
            }
            return encrypted;
        }

        public static byte[] DecBytes2(byte[] byteText)
        {
            var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            string sIV = "1d2c793dedc237897269859b34d31d93";
            string sKey = "3d6b59e8c5623ce4ff7c165995b209e7f03461ec057ca33a5cd1559d01e5682b";
            aes.Key = Encoders.Hex.DecodeData(sKey);
            aes.IV = Encoders.Hex.DecodeData(sIV);
            byte[] dcrypted;
            using (MemoryStream ms = new MemoryStream())
            {

                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(byteText, 0, byteText.Length);
                    cs.FlushFinalBlock();
                }
                dcrypted = ms.ToArray();
            }
            return dcrypted;
        }

        public static byte[] EncryptBytes(byte[] byteText)
        {

            var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;

            // This Salt & IV was ported in from Biblepay-QT
            string sSalt = "eb5a781ea9da2ef36";
            string sKey = "biblepay";
            byte[] bytesSalt = System.Text.Encoding.ASCII.GetBytes(sSalt);
            byte[] bytesKey = System.Text.Encoding.ASCII.GetBytes(sKey);

            // These static bytes were ported in from Biblepay-QT, because OpenSSL uses a proprietary method to create the 256 bit AES-CBC key: EVP_BytesToKey(EVP_aes_256_cbc(), EVP_sha512()
            string sAdvancedKey = "98,-5,23,119,-28,-99,-5,90,62,-63,82,39,63,-67,-85,37,-29,-65,97,80,57,-24,71,67,119,14,-67,12,-96,99,-84,-97";
            string sIV = "29,44,121,61,-19,-62,55,-119,114,105,-123,-101,52,-45,29,-109";
            var vKey = sAdvancedKey.Split(new string[] { "," }, StringSplitOptions.None);
            var vIV = sIV.Split(new string[] { "," }, StringSplitOptions.None);
            byte[] myBytedKey = new byte[32];
            byte[] myBytedIV = new byte[16];
        
            for (int i = 0; i < vKey.Length; i++)
            {
                int iMyKey = (int)BibleHash.cdbl(vKey[i], 0);
                myBytedKey[i] = (byte)(iMyKey + 0);
            }
            for (int i = 0; i < vIV.Length; i++)
            {
                int iMyIV = (int)BibleHash.cdbl(vIV[i], 0);
                myBytedIV[i] = (byte)(iMyIV + 0);
            }

            aes.Key = myBytedKey;
            aes.IV = myBytedIV;
            byte[] encrypted;
            HexEncoder he = new HexEncoder();

            string keyHex = he.EncodeData(myBytedKey);
            string keyIV = he.EncodeData(myBytedIV);
            string sBytedKeyHex = BibleHash.ByteArrayToHexString(myBytedKey);
            string sBytedIVHex = BibleHash.ByteArrayToHexString(myBytedIV);

            using (MemoryStream ms = new MemoryStream())
            {

                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(byteText, 0, byteText.Length);
                    cs.FlushFinalBlock();
                }
                encrypted = ms.ToArray();
            }
            return encrypted;
        }


        string BibleMD5(string sData)
        {
            return GetMd5String(sData);
        }


        int HTD(string sOctet)
        {
            int i1 = ATD((int)sOctet[0]);
            int i2 = ATD((int)sOctet[1]);
            int i3 = ATD((int)sOctet[2]);
            int i4 = ATD((int)sOctet[3]);
            int iOut = (i1 * 16 * 16 * 16) + (i2 * 16 * 16) + (i3 * 16) + (i4);
            return iOut;
        }

        // The various BibleHash functions for BiblePay - writen July 2017
        // Given an input hash provide a bible hash based on the chained verses found in the bits of the input hash.
        // Encryption step: Decrease the possibility of porting to a GPU
        // In the future, we may wish to add a deterministic historical TXID lookup in this algorithm to require full node hashing
        // (and possibly a node-node token exchange during mining).  The Historical TXID lookup requires a change in ReadBlockFromDisk - since blocks are not read in order, the hash function will not have enough information to find the ancestor for the TXID, therefore it will fail, until a creative solution is developed.
        // The primary reasons we created biblehash: Decrease network heat (by discouraging a mining arms race while encouraging full nodes to participate) 
        // Full nodes must be online servicing the network to be paid a subsidy.  
        // The bible is also a useful reference for in-wallet features such as prayer pages, expanding chained bible verses, scrolling prayers, pointers to prayers, etc.

        string RoundToString(double d, int place)
        {
            double e = Math.Round(d, place);
            string s = Convert.ToString(e);
            return s;
        }

        bool BibleEncrypt(string plaintext, ref string encrypted)
        {
            encrypted = "encrypted";
            return true;
        }

        byte[] ReverseArray(byte[] a)
        {
            byte[] baOut = new byte[a.Length];
            int iPos = a.Length;
            for (int i = 0; i < a.Length; i++)
            {
                iPos = iPos - 1;
                baOut[iPos] = a[i];
            }
            return baOut;
        }

        private static readonly long IMASK = 0xffffffffL;
        private int[] multiply(int[] x, int[] y, int[] z)
        {
            for (int i = z.Length - 1; i >= 0; i--)
            {
                long a = z[i] & IMASK;
                long value = 0;

                for (int j = y.Length - 1; j >= 0; j--)
                {
                    value += a * (y[j] & IMASK) + (x[i + j + 1] & IMASK);

                    x[i + j + 1] = (int)value;

                    value = (long)((ulong)value >> 32);
                }

                x[i] = (int)value;
            }

            return x;
        }


        public string ZeroPad(byte[] b)
        {
            string d256 = "0000000000" + Encoders.Hex.EncodeData(b);
            string dRight = d256.Substring(d256.Length - 64, 64);
            return dRight;
        }

        public uint256 MultiplyUint256(uint256 hSource, int iFactor)
        {
            arith256 a256 = new arith256(hSource);
            a256.MultiplyStratis((uint)iFactor);
            uint256 b256 = a256.GetUint256();
            byte[] c256in = b256.ToBytes();
            byte[] c256 = ReverseArray(c256in);
            string d256 = Encoders.Hex.EncodeData(c256);
            BouncyCastle.Math.BigInteger bi2000Divisor = new BouncyCastle.Math.BigInteger(RoundToString(1260, 0));
            BouncyCastle.Math.BigInteger b2000 = new BouncyCastle.Math.BigInteger(d256, 16);
            b2000 = b2000.Divide(bi2000Divisor);
            string sFinal = ZeroPad(b2000.ToByteArrayUnsigned());
            uint256 hFinal = new uint256(sFinal);
            return hFinal;
        }

        public uint256 GetBibleHash(uint256 hash, int nBlockTime, int nPrevBlockTime, bool bMining, int nPrevHeight, bool f7000, bool f8000, bool f9000, bool fTitheBlocksActive, int nNonce)
        {
            byte[] bytPlainText = hash.ToBytes();
            uint256 e = new uint256(0);

            byte[] bytEncrypted = EncryptBytes(bytPlainText);  //<-- so we need to get hash.ToBytes(),first, then encrypt that
            string sEncHex = ByteArrayToHexString(bytEncrypted);



            string sBase642 = Convert.ToBase64String(bytEncrypted);

            string sHash = BibleMD5(Convert.ToBase64String(bytEncrypted));
            string sVerses = "";
            string[] sOctets = new string[8];
            int iOctetNumber = 0;
            string rn = ((char)13).ToString() + ((char)10).ToString();

            for (int i = 0; i < (int)sHash.Length; i = i + 4)
            {
                string sOctet = sHash.Substring(i, 4);
                int iVerse = (int)(HTD(sOctet) * iVerseFactor);
                sOctets[iOctetNumber] = kjv.b[iVerse] + rn;
                iOctetNumber++;
            }
            sVerses = sOctets[0] + sOctets[1] + sOctets[2] + sOctets[3] + sOctets[4] + sOctets[5] + sOctets[6] + sOctets[7];
            if (nPrevHeight > SPORK8_HEIGHT) sVerses += " (" + RoundToString(nNonce, 0) + ")";
            string smd1 = BibleMD5(sVerses);

            byte[] vchVerses = System.Text.Encoding.ASCII.GetBytes(smd1);

            if (!bMining) sChainedVerses = sVerses;
            // Return a hash based on the work
            string vchMidVerses = Encoders.Hex.EncodeData(vchVerses);

            uint256 h = Script.SerializeHash(smd1);
            if (f9000)
            {
                // Let them solve a math problem here using a non-deterministic number of BiblePay hashes
                string sMath256 = sHash.Substring(0, 2);
                int iOpCount = (int)(ConvertHexToDouble("0x" + sMath256));
                for (int iCt = 0; iCt <= iOpCount; iCt++)
                {
                    h = HashBiblePay.Instance.Hash(h.ToBytes());
                }
            }
            else if (f8000)
            {
                // Let them solve a math problem here using a non-deterministic number of X11 hashes
                string sMath4096 = sHash.Substring(0, 3);
                int iOpCount = (int)(ConvertHexToDouble("0x" + sMath4096) * .5);
                for (int iCt = 0; iCt <= iOpCount; iCt++)
                {
                    h = Script.SerializeHash(h.ToString());
                }
            }
            // End of f8000 & f9000

            int nElapsed = nBlockTime - nPrevBlockTime;

            int nLateBlockThreshhold = 0;

            nLateBlockThreshhold = (f7000 || f8000) ? 30 * 60 : 60 * 60; // This test passes if the chain happens to get stuck (hopefully this never happens), but just in case, if there are no blocks generated in an hour, the difficulty drops 95%.  Hacking is prevented by blocking timestamps newer than 15 minutes of network adjusted time.
            if (fProd)
            {
                if (fDistributedComputingEnabled && nPrevHeight > F11000_CUTOVER_HEIGHT_PROD) nLateBlockThreshhold = 16;
            }
            else
            {
                if (!fProd && fDistributedComputingEnabled) nLateBlockThreshhold = 16;
            }

            if ((fTitheBlocksActive && nPrevHeight > 500 && ((nPrevHeight + 1) % TITHE_MODULUS) == 0) || nPrevHeight == 0) nElapsed = nLateBlockThreshhold + 1; // Prevents Miners from block skipping the tithe block
            int nDivisor = 0;

            if (nPrevHeight > F10000_CUTOVER_HEIGHT)
            {
                nDivisor = (nElapsed > nLateBlockThreshhold) ? 8400 : 1777;
            }
            else
            {
                nDivisor = (nElapsed > nLateBlockThreshhold) ? 8400 : 1260;
            }

            uint256 hFinal = MultiplyUint256(h, 420);
            return hFinal;

        }
    }
}
