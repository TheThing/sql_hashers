extern alias ExternalArgon2;
extern alias SafeArgon2;

using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace ConsoleTest
{
    internal class Program
    {
        const bool debug = false;
        static void Main(string[] args)
        {
            Console.WriteLine("");
            TestExternalArgon2();
            Console.WriteLine("");
            Console.WriteLine("");
            TestSafeArgon2();
            Console.WriteLine("");
            Console.WriteLine("");

            Console.ReadKey();
        }

        static void TestExternalArgon2()
        {
            string hash;

            var sw = new Stopwatch();
            sw.Start();
            ExternalArgon2.MsSqlArgon2.Argon2id_hash("Hello", out hash);
            sw.Stop();

            //                                            =
            Console.WriteLine("ExternalArgon2 regular     = {0}", sw.Elapsed);
            if (debug)
            {
                Console.WriteLine(hash);
                Console.WriteLine("Works? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works in Safe? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello1", hash));
                Console.WriteLine("Works in Safe? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello1", hash));
                Console.WriteLine();
            }

            sw = new Stopwatch();
            sw.Start();
            
            ExternalArgon2.MsSqlArgon2.Argon2id_hash_custom("Hello", 1, 4, 1, 33, out hash);
            sw.Stop();

            //                                            =
            Console.WriteLine("ExternalArgon2 fast        = {0}", sw.Elapsed);
            if (debug)
            {
                Console.WriteLine(hash);
                Console.WriteLine("Works? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works in Safe? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine();
            }

            return;


            sw = new Stopwatch();
            sw.Start();
            ExternalArgon2.MsSqlArgon2.Argon2id_hash_custom("Hello", 1, 128, 8, 32, out hash);
            sw.Stop();

            //                                            =
            Console.WriteLine("ExternalArgon2 slower      = {0}", sw.Elapsed);
            if (debug)
            {
                Console.WriteLine(hash);
                Console.WriteLine("Works? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works in Safe? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine();
            }


            sw = new Stopwatch();
            sw.Start();
            ExternalArgon2.MsSqlArgon2.Argon2id_hash_custom("Hello", 1, 128, 15, 32, out hash);
            sw.Stop();

            //                                            =
            Console.WriteLine("ExternalArgon2 slow        = {0}", sw.Elapsed);
            if (debug)
            {
                Console.WriteLine(hash);
                Console.WriteLine("Works? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works in Safe? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine();
            }


            sw = new Stopwatch();
            sw.Start();
            ExternalArgon2.MsSqlArgon2.Argon2id_hash_custom("Hello", 4, 128, 15, 32, out hash);
            sw.Stop();

            //                                            =
            Console.WriteLine("ExternalArgon2 slow parall = {0}", sw.Elapsed);
            if (debug)
            {
                Console.WriteLine(hash);
                Console.WriteLine("Works? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works in Safe? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine();
            }
        }

        static void TestSafeArgon2()
        {
            string hash;

            var sw = new Stopwatch();
            sw.Start();
            SafeArgon2.MsSqlArgon2.Argon2id_hash("Hello", out hash);
            sw.Stop();

            //                                            =
            Console.WriteLine("SafeArgon2 regular         = {0}", sw.Elapsed);
            if (debug)
            {
                Console.WriteLine(hash);
                Console.WriteLine("Works? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works in External? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine();
            }

            sw = new Stopwatch();
            sw.Start();
            SafeArgon2.MsSqlArgon2.Argon2id_hash_custom("Hello", 1, 4, 1, 33, out hash);
            sw.Stop();

            //                                            =
            Console.WriteLine("SafeArgon2 fast            = {0}", sw.Elapsed);
            if (debug)
            {
                Console.WriteLine(hash);
                Console.WriteLine("Works? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works in External? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine();
            }

            return;

            sw = new Stopwatch();
            sw.Start();
            SafeArgon2.MsSqlArgon2.Argon2id_hash_custom("Hello", 1, 128, 8, 32, out hash);
            sw.Stop();

            //                                            =
            Console.WriteLine("SafeArgon2 slower          = {0}", sw.Elapsed);
            if (debug)
            {
                Console.WriteLine(hash);
                Console.WriteLine("Works? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works in External? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine();
            }


            sw = new Stopwatch();
            sw.Start();
            SafeArgon2.MsSqlArgon2.Argon2id_hash_custom("Hello", 1, 128, 15, 32, out hash);
            sw.Stop();

            //                                            =
            Console.WriteLine("SafeArgon2 slow            = {0}", sw.Elapsed);
            if (debug)
            {
                Console.WriteLine(hash);
                Console.WriteLine("Works? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works in External? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine();
            }


            sw = new Stopwatch();
            sw.Start();
            SafeArgon2.MsSqlArgon2.Argon2id_hash_custom("Hello", 4, 128, 15, 32, out hash);
            sw.Stop();

            //                                            =
            Console.WriteLine("SafeArgon2 slow parallel   = {0}", sw.Elapsed);
            if (debug)
            {
                Console.WriteLine(hash);
                Console.WriteLine("Works? {0}", SafeArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine("Works in External? {0}", ExternalArgon2.MsSqlArgon2.Argon2id_verify("Hello", hash));
                Console.WriteLine();
            }

        }
    }
}
