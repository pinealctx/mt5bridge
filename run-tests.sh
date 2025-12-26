#!/bin/bash
# MT5Bridge Test Runner Script (Linux/macOS)

set -e

# Default parameters
MODE="all"
VERBOSITY="minimal"
NO_BUILD=false
COVERAGE=false
FILTER=""

# Display help information
show_help() {
    cat << EOF
MT5Bridge Test Runner Script

Usage: ./run-tests.sh [options]

Options:
    -m, --mode <mode>        Test mode (all|core|manager|benchmarks|atomic|timing|text|collections|logging|filter)
    -f, --filter <filter>    Custom test filter (used when mode=filter)
    -v, --verbosity <level>  Output verbosity (quiet|minimal|normal|detailed)
    -n, --no-build          Skip build, run tests directly
    -c, --coverage          Generate code coverage report
    -h, --help              Display this help information

Examples:
    ./run-tests.sh
    ./run-tests.sh -m core
    ./run-tests.sh -m manager
    ./run-tests.sh -m benchmarks
    ./run-tests.sh -m atomic
    ./run-tests.sh -c

EOF
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -m|--mode)
            MODE="$2"
            shift 2
            ;;
        -f|--filter)
            FILTER="$2"
            shift 2
            ;;
        -v|--verbosity)
            VERBOSITY="$2"
            shift 2
            ;;
        -n|--no-build)
            NO_BUILD=true
            shift
            ;;
        -c|--coverage)
            COVERAGE=true
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            echo "Unknown parameter: $1"
            show_help
            exit 1
            ;;
    esac
done

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Define project paths
CORE_PROJECT="$SCRIPT_DIR/MT5Bridge.Core.Tests/MT5Bridge.Core.Tests.csproj"
MANAGER_PROJECT="$SCRIPT_DIR/MT5Bridge.Manager.Tests/MT5Bridge.Manager.Tests.csproj"
BENCHMARK_PROJECT="$SCRIPT_DIR/MT5Bridge.Benchmarks/MT5Bridge.Benchmarks.csproj"

# Determine which projects to run
TARGET_PROJECTS=()
if [ "$MODE" = "all" ]; then
    TARGET_PROJECTS=("core" "manager")
elif [ "$MODE" = "core" ] || [ "$MODE" = "atomic" ] || [ "$MODE" = "timing" ] || [ "$MODE" = "text" ] || [ "$MODE" = "collections" ] || [ "$MODE" = "logging" ] || [ "$MODE" = "filter" ]; then
    TARGET_PROJECTS=("core")
elif [ "$MODE" = "manager" ]; then
    TARGET_PROJECTS=("manager")
elif [ "$MODE" = "benchmarks" ]; then
    TARGET_PROJECTS=("benchmarks")
fi

# Build test filter for Core project
case $MODE in
    atomic)
        TEST_FILTER="FullyQualifiedName~MT5Bridge.Core.Tests.Atomic"
        ;;
    timing)
        TEST_FILTER="FullyQualifiedName~MT5Bridge.Core.Tests.Timing"
        ;;
    text)
        TEST_FILTER="FullyQualifiedName~MT5Bridge.Core.Tests.Text"
        ;;
    collections)
        TEST_FILTER="FullyQualifiedName~MT5Bridge.Core.Tests.Collections"
        ;;
    logging)
        TEST_FILTER="FullyQualifiedName~MT5Bridge.Core.Tests.Logging"
        ;;
    filter)
        TEST_FILTER="$FILTER"
        ;;
    *)
        TEST_FILTER=""
        ;;
esac

# Display run information
echo "================================================"
echo " MT5Bridge Test Runner"
echo "================================================"
echo "Mode:      $MODE"
echo "Projects:  ${TARGET_PROJECTS[*]}"
if [ -n "$TEST_FILTER" ]; then
    echo "过滤器:   $TEST_FILTER"
fi
echo "详细程度: $VERBOSITY"
if [ "$COVERAGE" = true ]; then
    echo "覆盖率:   启用"
fi
echo "================================================"
echo ""

# Record start time
START_TIME=$(date +%s)

for PROJ in "${TARGET_PROJECTS[@]}"; do
    case $PROJ in
        core)
            PROJ_PATH="$CORE_PROJECT"
            ;;
        manager)
            PROJ_PATH="$MANAGER_PROJECT"
            ;;
        benchmarks)
            PROJ_PATH="$BENCHMARK_PROJECT"
            ;;
    esac

    if [ ! -f "$PROJ_PATH" ]; then
        echo "Warning: Project not found: $PROJ_PATH"
        continue
    fi

    echo ">>> Running $PROJ ..."

    if [ "$PROJ" = "benchmarks" ]; then
        dotnet run -c Release --project "$PROJ_PATH"
    else
        TEST_CMD="dotnet test \"$PROJ_PATH\" --logger \"console;verbosity=$VERBOSITY\""
        
        if [ "$NO_BUILD" = true ]; then
            TEST_CMD="$TEST_CMD --no-build"
        fi

        if [ "$PROJ" = "core" ] && [ -n "$TEST_FILTER" ]; then
            TEST_CMD="$TEST_CMD --filter \"$TEST_FILTER\""
        fi

        if [ "$COVERAGE" = true ]; then
            RESULTS_DIR="$SCRIPT_DIR/TestResults"
            TEST_CMD="$TEST_CMD --collect:\"XPlat Code Coverage\" --results-directory \"$RESULTS_DIR\""
        fi

        eval $TEST_CMD
    fi
done

# Calculate execution time
END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

# Display execution time
echo ""
echo "================================================"
echo -e "\033[32mTotal execution time: ${DURATION} seconds\033[0m"
echo "================================================"

exit 0
