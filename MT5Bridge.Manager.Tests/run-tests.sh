#!/usr/bin/env bash
# Runs MT5Bridge.MT5.Core.Tests with coverage reporting.
# 运行 MT5Bridge.MT5.Core.Tests 并生成覆盖率报告。

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR"
PROJECT_FILE="$PROJECT_DIR/MT5Bridge.MT5.Core.Tests.csproj"

# Parse arguments
COVERAGE=false
FILTER=""
VERBOSE=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--coverage)
            COVERAGE=true
            shift
            ;;
        -f|--filter)
            FILTER="$2"
            shift 2
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  -c, --coverage    Enable code coverage collection"
            echo "  -f, --filter      Filter tests by name"
            echo "  -v, --verbose     Show detailed test output"
            echo "  -h, --help        Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0"
            echo "  $0 --coverage"
            echo "  $0 --filter DealModel"
            echo "  $0 --coverage --verbose"
            exit 0
            ;;
        *)
            echo -e "${RED}❌ Unknown option: $1${NC}"
            exit 1
            ;;
    esac
done

echo -e "${CYAN}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${CYAN}  MT5Bridge.MT5.Core.Tests${NC}"
echo -e "${CYAN}═══════════════════════════════════════════════════════════════${NC}"
echo ""

# Check if project file exists
if [ ! -f "$PROJECT_FILE" ]; then
    echo -e "${RED}❌ Error: Project file not found: $PROJECT_FILE${NC}"
    exit 1
fi

# Build test arguments
TEST_ARGS=(
    "test"
    "$PROJECT_FILE"
    "--configuration" "Debug"
    "--no-build"
    "--nologo"
)

# Add filter if specified
if [ -n "$FILTER" ]; then
    TEST_ARGS+=("--filter" "$FILTER")
    echo -e "${YELLOW}🔍 Filter: $FILTER${NC}"
    echo ""
fi

# Add verbosity
if [ "$VERBOSE" = true ]; then
    TEST_ARGS+=("--verbosity" "detailed")
else
    TEST_ARGS+=("--verbosity" "normal")
fi

# Add coverage if specified
if [ "$COVERAGE" = true ]; then
    echo -e "${GREEN}📊 Code coverage enabled${NC}"
    echo ""
    
    TEST_ARGS+=("--collect:XPlat Code Coverage")
    TEST_ARGS+=("--")
    TEST_ARGS+=("DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover")
fi

# Run build first
echo -e "${YELLOW}🔨 Building project...${NC}"
dotnet build "$PROJECT_FILE" --configuration Debug --nologo

if [ $? -ne 0 ]; then
    echo ""
    echo -e "${RED}❌ Build failed${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}✅ Build successful${NC}"
echo ""

# Run tests
echo -e "${YELLOW}🧪 Running tests...${NC}"
echo ""

dotnet "${TEST_ARGS[@]}"
TEST_EXIT_CODE=$?

echo ""

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}✅ All tests passed!${NC}"
else
    echo -e "${RED}❌ Some tests failed${NC}"
fi

# Display coverage results if enabled
if [ "$COVERAGE" = true ] && [ $TEST_EXIT_CODE -eq 0 ]; then
    echo ""
    echo -e "${CYAN}═══════════════════════════════════════════════════════════════${NC}"
    echo -e "${CYAN}  Coverage Report${NC}"
    echo -e "${CYAN}═══════════════════════════════════════════════════════════════${NC}"
    echo ""
    
    # Find coverage file
    COVERAGE_FILE=$(find "$PROJECT_DIR" -name "coverage.opencover.xml" -type f | head -n 1)
    
    if [ -n "$COVERAGE_FILE" ]; then
        echo -e "${GREEN}📊 Coverage file: $COVERAGE_FILE${NC}"
        echo ""
        echo -e "${YELLOW}To view detailed coverage, install reportgenerator:${NC}"
        echo -e "${GRAY}  dotnet tool install -g dotnet-reportgenerator-globaltool${NC}"
        echo ""
        echo -e "${YELLOW}Then generate HTML report:${NC}"
        echo -e "${GRAY}  reportgenerator -reports:$COVERAGE_FILE -targetdir:coverage-report${NC}"
        echo ""
    else
        echo -e "${YELLOW}⚠️  Warning: Coverage file not found${NC}"
    fi
fi

echo ""
echo -e "${CYAN}═══════════════════════════════════════════════════════════════${NC}"
echo ""

exit $TEST_EXIT_CODE
