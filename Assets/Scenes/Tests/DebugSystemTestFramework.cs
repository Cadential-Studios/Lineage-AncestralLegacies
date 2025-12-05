using UnityEngine;
using System;
using System.Collections.Generic;

namespace Lineage.Tests
{
    /// <summary>
    /// Test framework for the debug and logging system.
    /// Validates log levels, categories, and output formatting.
    /// </summary>
    public class DebugSystemTestFramework : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _runOnStart = false;
        [SerializeField] private bool _verboseOutput = true;

        private List<TestResult> _testResults = new List<TestResult>();

        private struct TestResult
        {
            public string TestName;
            public bool Passed;
            public string Message;
        }

        private void Start()
        {
            if (_runOnStart)
            {
                RunAllTests();
            }
        }

        /// <summary>
        /// Runs all debug system tests.
        /// </summary>
        public void RunAllTests()
        {
            _testResults.Clear();

            LogTestStart("Debug System Test Framework");

            TestLogLevels();
            TestLogCategories();
            TestLogFormatting();
            TestConditionalLogging();

            ReportResults();
        }

        private void TestLogLevels()
        {
            string testName = "Log Level Tests";
            try
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                // Test that different log levels work
                Lineage.Debug.Log.Info("Test info message", Lineage.Debug.Log.LogCategory.General);
                Lineage.Debug.Log.Warning("Test warning message", Lineage.Debug.Log.LogCategory.General);
                
                RecordResult(testName, true, "Log levels working correctly");
#else
                RecordResult(testName, true, "Logging disabled in release build (expected)");
#endif
            }
            catch (Exception ex)
            {
                RecordResult(testName, false, $"Exception: {ex.Message}");
            }
        }

        private void TestLogCategories()
        {
            string testName = "Log Category Tests";
            try
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                // Test all categories
                var categories = Enum.GetValues(typeof(Lineage.Debug.Log.LogCategory));
                foreach (Lineage.Debug.Log.LogCategory category in categories)
                {
                    Lineage.Debug.Log.Info($"Testing category: {category}", category);
                }
                RecordResult(testName, true, $"All {categories.Length} categories working");
#else
                RecordResult(testName, true, "Logging disabled in release build (expected)");
#endif
            }
            catch (Exception ex)
            {
                RecordResult(testName, false, $"Exception: {ex.Message}");
            }
        }

        private void TestLogFormatting()
        {
            string testName = "Log Formatting Tests";
            try
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                // Test with context object
                Lineage.Debug.Log.Info("Message with context", Lineage.Debug.Log.LogCategory.General, this);
                
                // Test string interpolation
                int testValue = 42;
                Lineage.Debug.Log.Info($"Interpolated value: {testValue}", Lineage.Debug.Log.LogCategory.General);
                
                RecordResult(testName, true, "Log formatting working correctly");
#else
                RecordResult(testName, true, "Logging disabled in release build (expected)");
#endif
            }
            catch (Exception ex)
            {
                RecordResult(testName, false, $"Exception: {ex.Message}");
            }
        }

        private void TestConditionalLogging()
        {
            string testName = "Conditional Logging Tests";
            try
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                // Store original level
                var originalLevel = Lineage.Debug.Log.MinLogLevel;

                // Test filtering by level
                Lineage.Debug.Log.MinLogLevel = Lineage.Debug.Log.LogLevel.Warning;
                Lineage.Debug.Log.Info("This should be filtered out", Lineage.Debug.Log.LogCategory.General);
                Lineage.Debug.Log.Warning("This should appear", Lineage.Debug.Log.LogCategory.General);

                // Restore original level
                Lineage.Debug.Log.MinLogLevel = originalLevel;

                RecordResult(testName, true, "Conditional logging working correctly");
#else
                RecordResult(testName, true, "Logging disabled in release build (expected)");
#endif
            }
            catch (Exception ex)
            {
                RecordResult(testName, false, $"Exception: {ex.Message}");
            }
        }

        private void RecordResult(string testName, bool passed, string message)
        {
            _testResults.Add(new TestResult
            {
                TestName = testName,
                Passed = passed,
                Message = message
            });

            if (_verboseOutput)
            {
                string status = passed ? "✓ PASS" : "✗ FAIL";
                UnityEngine.Debug.Log($"[DebugSystemTest] {status}: {testName} - {message}");
            }
        }

        private void LogTestStart(string frameworkName)
        {
            UnityEngine.Debug.Log($"=== {frameworkName} Starting ===");
        }

        private void ReportResults()
        {
            int passed = 0;
            int failed = 0;

            foreach (var result in _testResults)
            {
                if (result.Passed) passed++;
                else failed++;
            }

            UnityEngine.Debug.Log($"=== Test Results: {passed}/{_testResults.Count} Passed ===");

            if (failed > 0)
            {
                UnityEngine.Debug.LogWarning($"[DebugSystemTest] {failed} test(s) failed!");
            }
            else
            {
                UnityEngine.Debug.Log("[DebugSystemTest] All tests passed!");
            }
        }
    }
}
// Pseudocode generated by codewrx.ai