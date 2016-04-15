using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OcularPlane.InternalModels;
using OcularPlane.Models;

namespace OcularPlane
{
    static class MethodExpressionParser
    {
        public static TrackedMethod ParseToReference(Expression<Action> methodExpression, string storedName)
        {
            if (methodExpression == null) throw new ArgumentNullException(nameof(methodExpression));
            if (string.IsNullOrWhiteSpace(storedName)) throw new ArgumentNullException(storedName);

            var member = methodExpression.Body as MethodCallExpression;
            if (member == null)
            {
                throw new ArgumentException("Expression is not a method");
            }

            var memberInfo = member.Method;
            var attachedObject = GetClassObject(methodExpression);
            if (attachedObject == null)
            {
                throw new InvalidOperationException("Could not find attached object in expression tree. Methods passed in with 'new Class().Method()' is not supported");
            }

            var methodId = Guid.NewGuid();
            return new TrackedMethod
            {
                MethodId = methodId,
                RelvantObject = attachedObject,
                MethodInfo = memberInfo,
                Reference = new MethodReference
                {
                    MethodId = methodId,
                    Name = storedName,
                    Parameters = memberInfo.GetParameters()
                    .Select(x => new ParameterReference
                    {
                        TypeName = x.ParameterType.FullName,
                        ParameterName = x.Name
                    })
                    .ToArray()
                }
            };
        }

        // From http://stackoverflow.com/a/3607659/231002
        private static object GetClassObject(Expression<Action> expression)
        {
            // The expression is a lambda expression with a method call body.
            var lambda = (LambdaExpression)expression;
            var methodCall = (MethodCallExpression)lambda.Body;
            // The method is called on a member of some instance.
            var member = (MemberExpression)methodCall.Object;
            // The member expression contains an instance of the anonymous class that
            // defines the member...
            var constant = (ConstantExpression)member.Expression;
            var anonymousClassInstance = constant.Value;
            // ...and the member itself.
            var calledClassField = (FieldInfo)member.Member;
            // With an instance of the anonymous class and the field, we can get its value.
            return calledClassField.GetValue(anonymousClassInstance);
        }
    }
}
