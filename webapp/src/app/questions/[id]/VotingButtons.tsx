import {Button, Link } from "@heroui/react";
import {ArrowDownCircleIcon, ArrowUpCircleIcon} from "@heroicons/react/24/solid";
import {CheckIcon} from "@heroicons/react/24/outline";

type Props = {
    accepted?: boolean,
}

export default function VotingButtons({accepted}: Props) {
    return (
        <div>
            <div className='shrink-0 flex flex-col gap-3 items-center justify-start mt-4'>
                <Link>
                    <Button>
                        <ArrowUpCircleIcon/>
                    </Button>
                </Link>
                <span className='text-xl font-semibold'>0</span>
                <Link>
                    <Button>
                        <ArrowDownCircleIcon/>
                    </Button>
                </Link>
                {accepted && (
                    <Button isIconOnly={true}>
                        <CheckIcon className="size-12 text-success" strokeWidth={4} />
                    </Button>
                )}
            </div>
        </div>
    );
}
