#!/bin/bash
# MT5Bridge.Core 测试运行脚本 (Linux/macOS)

set -e

# 默认参数
MODE="all"
VERBOSITY="minimal"
NO_BUILD=false
COVERAGE=false
FILTER=""

# 显示帮助信息
show_help() {
    cat << EOF
MT5Bridge.Core 测试运行脚本

用法: ./run-tests.sh [选项]

选项:
    -m, --mode <mode>        测试模式 (all|atomic|timing|text|collections|logging|filter)
    -f, --filter <filter>    自定义测试过滤器 (当 mode=filter 时使用)
    -v, --verbosity <level>  输出详细程度 (quiet|minimal|normal|detailed)
    -n, --no-build          跳过构建，直接运行测试
    -c, --coverage          生成代码覆盖率报告
    -h, --help              显示此帮助信息

示例:
    ./run-tests.sh
    ./run-tests.sh -m atomic
    ./run-tests.sh -m filter -f "FullyQualifiedName~BackoffTimer"
    ./run-tests.sh -v detailed
    ./run-tests.sh -c

EOF
}

# 解析命令行参数
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
            echo "未知参数: $1"
            show_help
            exit 1
            ;;
    esac
done

# 获取脚本所在目录
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
TEST_PROJECT="$SCRIPT_DIR/MT5Bridge.Core.Tests/MT5Bridge.Core.Tests.csproj"

# 检查测试项目是否存在
if [ ! -f "$TEST_PROJECT" ]; then
    echo "错误: 测试项目不存在: $TEST_PROJECT"
    exit 1
fi

# 构建测试过滤器
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
    all|*)
        TEST_FILTER=""
        ;;
esac

# 构建测试命令
TEST_CMD="dotnet test \"$TEST_PROJECT\" --logger \"console;verbosity=$VERBOSITY\""

# 添加 NoBuild 参数
if [ "$NO_BUILD" = true ]; then
    TEST_CMD="$TEST_CMD --no-build"
fi

# 添加过滤器
if [ -n "$TEST_FILTER" ]; then
    TEST_CMD="$TEST_CMD --filter \"$TEST_FILTER\""
fi

# 添加覆盖率参数
if [ "$COVERAGE" = true ]; then
    RESULTS_DIR="$SCRIPT_DIR/TestResults"
    TEST_CMD="$TEST_CMD --collect:\"XPlat Code Coverage\" --results-directory \"$RESULTS_DIR\""
fi

# 显示运行信息
echo "================================================"
echo " MT5Bridge.Core 测试运行"
echo "================================================"
echo "模式:     $MODE"
if [ -n "$TEST_FILTER" ]; then
    echo "过滤器:   $TEST_FILTER"
fi
echo "详细程度: $VERBOSITY"
if [ "$COVERAGE" = true ]; then
    echo "覆盖率:   启用"
fi
echo "================================================"
echo ""

# 记录开始时间
START_TIME=$(date +%s)

# 运行测试
eval $TEST_CMD
EXIT_CODE=$?

# 计算执行时间
END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

# 显示执行时间
echo ""
echo "================================================"
if [ $EXIT_CODE -eq 0 ]; then
    echo -e "\033[32m测试执行时间: ${DURATION} 秒\033[0m"
else
    echo -e "\033[31m测试执行时间: ${DURATION} 秒\033[0m"
fi
echo "================================================"

# 如果启用了覆盖率，显示结果位置
if [ "$COVERAGE" = true ] && [ $EXIT_CODE -eq 0 ]; then
    echo ""
    echo "覆盖率报告已生成到: $RESULTS_DIR"
    echo "提示: 使用 ReportGenerator 工具可以生成 HTML 报告:"
    echo "  dotnet tool install -g dotnet-reportgenerator-globaltool"
    echo "  reportgenerator -reports:\"$RESULTS_DIR/**/coverage.cobertura.xml\" -targetdir:\"$RESULTS_DIR/html\" -reporttypes:Html"
fi

exit $EXIT_CODE
