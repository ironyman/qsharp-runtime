﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;

using Microsoft.Quantum.Simulation.Common;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators.QCTraceSimulators;
using Microsoft.Quantum.Tests.CoreOperations;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Quantum.Simulation.Simulators.Tests
{
    using Helper = Microsoft.Quantum.Simulation.Simulators.Tests.OperationsTestHelper;

    public class CoreTests
    {
        private readonly ITestOutputHelper output;

        public CoreTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Borrowing()
        {
            Helper.RunWithMultipleSimulators((s) =>
            {
                var tracker = new StartTracker(s);

                var q0 = new FreeQubit(0) as Qubit;
                var q1 = new FreeQubit(1) as Qubit;
                var q2 = new FreeQubit(2) as Qubit;
                var q3 = new FreeQubit(3) as Qubit;
                var q4 = new FreeQubit(4) as Qubit;
                var q5 = new FreeQubit(5) as Qubit;
                var q6 = new FreeQubit(6) as Qubit;
                var q7 = new FreeQubit(7) as Qubit;
                var q8 = new FreeQubit(8) as Qubit;

                BorrowingTest.Run(s).Wait();

                var tracer = Helper.GetTracer<(long, Qubit)>(s);

                var testOne = new System.Action<int, OperationFunctor, (int, Qubit)>((callsCount, variant, info) =>
                {
                    var (available, q) = info;
                    Assert.Equal(callsCount, tracer.Log.GetNumberOfCalls(variant, (available, q)));
                });

                var testOneBody = new System.Action<int, (int, Qubit)>((callsCount, info) => testOne(callsCount, OperationFunctor.Body, info));
                var testOneAdjoint = new System.Action<int, (int, Qubit)>((callsCount, info) => testOne(callsCount, OperationFunctor.Adjoint, info));
                var testOneCtrl = new System.Action<int, (int, Qubit)>((callsCount, info) => testOne(callsCount, OperationFunctor.Controlled, info));
                var testOneCtrlAdj = new System.Action<int, (int, Qubit)>((callsCount, info) => testOne(callsCount, OperationFunctor.ControlledAdjoint, info));

                testOneBody(6, (0, q5));
                testOneBody(6, (0, q6));
                testOneBody(1, (1, q0));
                testOneBody(1, (1, q1));
                testOneBody(1, (1, q4));
                testOneBody(1, (1, q5));
                testOneBody(2, (1, q6));
                testOneBody(2, (2, q2));
                testOneBody(2, (2, q3));
                testOneBody(1, (3, q0));
                testOneBody(1, (3, q1));
                testOneBody(1, (3, q2));
                testOneBody(3, (3, q3));
                testOneBody(2, (4, q0));
                testOneBody(1, (4, q1));
                testOneBody(3, (4, q2));
                testOneBody(5, (5, q0));
                testOneBody(5, (5, q1));

                testOneAdjoint(3, (0, q5));
                testOneAdjoint(3, (0, q6));
                testOneAdjoint(1, (1, q0));
                testOneAdjoint(1, (1, q1));
                testOneAdjoint(1, (1, q4));
                testOneAdjoint(2, (1, q5));
                testOneAdjoint(1, (1, q6));
                testOneAdjoint(2, (2, q2));
                testOneAdjoint(2, (2, q3));
                testOneAdjoint(1, (3, q2));
                testOneAdjoint(1, (3, q3));
                testOneAdjoint(1, (4, q0));
                testOneAdjoint(1, (4, q1));
                testOneAdjoint(2, (4, q2));
                testOneAdjoint(2, (5, q0));
                testOneAdjoint(2, (5, q1));

                testOneCtrl(3, (0, q7));
                testOneCtrl(3, (0, q8));
                testOneCtrl(1, (1, q0));
                testOneCtrl(1, (1, q1));
                testOneCtrl(1, (1, q4));
                testOneCtrl(1, (1, q5));
                testOneCtrl(1, (1, q6));
                testOneCtrl(0, (2, q0));
                testOneCtrl(0, (2, q1));
                testOneCtrl(2, (2, q2));
                testOneCtrl(2, (2, q3));
                testOneCtrl(1, (3, q2));
                testOneCtrl(1, (3, q3));
                testOneCtrl(1, (4, q0));
                testOneCtrl(1, (4, q1));
                testOneCtrl(2, (4, q2));
                testOneCtrl(3, (5, q0));
                testOneCtrl(3, (5, q1));

                testOneCtrlAdj(3, (0, q7));
                testOneCtrlAdj(3, (0, q8));
                testOneCtrlAdj(1, (1, q4));
                testOneCtrlAdj(1, (1, q7));
                testOneCtrlAdj(2, (2, q2));
                testOneCtrlAdj(2, (2, q3));
                testOneCtrlAdj(1, (3, q2));
                testOneCtrlAdj(1, (3, q3));
                testOneCtrlAdj(1, (4, q0));
                testOneCtrlAdj(1, (4, q1));
                testOneCtrlAdj(2, (4, q2));
                testOneCtrlAdj(2, (5, q0));
                testOneCtrlAdj(2, (5, q1));

                Assert.Equal(20, tracker.GetNumberOfCalls("Microsoft.Quantum.Intrinsic.SWAP", OperationFunctor.Body));
                Assert.Equal(11, tracker.GetNumberOfCalls("Microsoft.Quantum.Intrinsic.SWAP", OperationFunctor.Adjoint));
                Assert.Equal(16, tracker.GetNumberOfCalls("Microsoft.Quantum.Intrinsic.SWAP", OperationFunctor.Controlled));
                Assert.Equal(13, tracker.GetNumberOfCalls("Microsoft.Quantum.Intrinsic.SWAP", OperationFunctor.ControlledAdjoint));
            });
        }

        [Fact]
        public void DumpState()
        {
            var expectedFiles = new string[]
            {
                "dumptest-start.txt",
                "dumptest-h.txt",
                "dumptest-former.txt",
                "dumptest-later.txt",
                "dumptest-one.txt",
                "dumptest-two.txt",
                "dumptest-entangled.txt",
                "dumptest-twoQubitsEntangled.txt",
                "dumptest-end.txt",
            };

            void RunOne(IOperationFactory s)
            {
                if (s is SimulatorBase sim)
                {
                    // OnLog defines action(s) performed when Q# test calls function Message
                    sim.OnLog += (msg) => { output.WriteLine(msg); };
                }

                foreach (var name in expectedFiles)
                {
                    if (File.Exists(name))
                    {
                        File.Delete(name);
                    }
                }

                if (File.Exists("()"))
                {
                    File.Delete("()");
                }

                SimpleDumpTest.Run(s).Wait();

                foreach (var name in expectedFiles)
                {
                    Assert.True(File.Exists(name), $"File {name} did not exist after running SimpleDumpTest on {s}.");
                }

                Assert.False(File.Exists("()"));
            }

            Helper.RunWithMultipleSimulators((s) => RunOne(s as IOperationFactory));
            RunOne(new QCTraceSimulator());
            RunOne(new ResourcesEstimator());
            RunOne(new QuantumSimulator());
        }


        [Fact]
        public void DumpOnLockedFile()
        {
            void RunOne(IOperationFactory s)
            {
                var messages = new List<string>();
                var filename = "locked-file.txt";

                if (s is SimulatorBase sim)
                {
                    // OnLog defines action(s) performed when Q# test calls function Message
                    sim.OnLog += messages.Add;
                }

                using (var lockedFile = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    LockedFileDumpTest.Run(s).Wait();

                    if (s is SimulatorBase)
                    {
                        Assert.Equal(3, messages.Count);
                        Assert.StartsWith($"[warning] Unable to write state to '{filename}' (The process cannot access the file", messages[0]);
                        Assert.StartsWith($"[warning] Unable to write state to '{filename}' (The process cannot access the file", messages[1]);
                        Assert.Equal("Done.", messages[2]);
                    }
                }
            }

            Helper.RunWithMultipleSimulators((s) => RunOne(s as IOperationFactory));
            RunOne(new QCTraceSimulator());
            RunOne(new ResourcesEstimator());
        }

        [Fact]
        public void ZeroQubits()
        {
            Helper.RunWithMultipleSimulators((s) =>
            {
                var tracer = Helper.GetTracer<string>(s);

                ZeroQubitsTest.Run(s).Wait();

                Assert.Equal(1, tracer.GetNumberOfCalls(OperationFunctor.Body, "zero"));
            });
        }

        [Fact]
        public void InterpolatedStrings()
        {
            Helper.RunWithMultipleSimulators((s) =>
            {
                Circuits.InterpolatedStringTest.Run(s).Wait(); // Throws if it doesn't succeed
            });
        }

        [Fact]
        public void RandomOperation()
        {
            Helper.RunWithMultipleSimulators((s) =>
            {
                Circuits.RandomOperationTest.Run(s).Wait(); // Throws if it doesn't succeed
            });
        }

        [Fact]
        public void BigInts()
        {
            Helper.RunWithMultipleSimulators((s) =>
            {
                Circuits.BigIntTest.Run(s).Wait(); // Throws if it doesn't succeed
            });
        }
    }
}
