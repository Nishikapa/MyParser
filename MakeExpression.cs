using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace MyParser
{
    static partial class Program
    {
        // Util ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        static BlockExpression ラムダをExpressionへ変換<TDelegate>(this Expression<TDelegate> eLambda, params Expression[] parameters)
        {
            if (eLambda.Parameters.Count() != parameters.Length)
            {
                throw new ArgumentException("ラムダ式のパラメータの数とparameters引数で渡されたパラメータの数が異なっています");
            }

            return Block(
                eLambda.Parameters,
                eLambda.Parameters.Zip(parameters, (p, r) => Assign(p, r))
                .Concat(new[] { eLambda.Body })
            );
        }

        static Expression<Action> 既定のツリーから式木を生成(this IEnumerable<既定> paramCalcChecks)
        {
            IEnumerable<Expression> 規定のコレクションをExpressionへ変換(IEnumerable<既定> kiteis) =>
                kiteis.Select
                (
                    kitei =>
                    {
                        switch (kitei)
                        {
                            case 繰り返し k:
                                {
                                    var expressions =
                                        規定のコレクションをExpressionへ変換(k.処理);

                                    var i = Parameter(typeof(int), "i");

                                    var endLoop = Label("EndLoop");

                                    var body = Block(
                                        new[] { i },
                                        Assign(i, Constant(k.繰り返し回数)),
                                        Loop(
                                            Block(
                                                new Expression[] {
                                                    IfThen(
                                                        LessThanOrEqual(i, Constant(0)),
                                                        Break(endLoop)
                                                    ),
                                                    SubtractAssign(i, Expression.Constant(1))
                                                }.Concat(expressions)
                                            ),
                                            endLoop
                                        )
                                    );

                                    return body;
                                }
                            case 表示 k:
                                {

                                    Expression<Action<string>> printLamnbda = (string s) =>
                                        Console.WriteLine(s);

                                    return ラムダをExpressionへ変換(printLamnbda, Constant(k.表示文字列));
                                }
                            default:
                                throw new Exception($"サポートされていないタイプです({kitei.GetType().ToString()})");
                        }
                    }

                    );

            return Lambda<Action>(Block(規定のコレクションをExpressionへ変換(paramCalcChecks)));
        }
    }
}
